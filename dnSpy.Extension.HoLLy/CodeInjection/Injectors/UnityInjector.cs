using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using dnSpy.Contracts.App;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    public class UnityInjector : IInjector
    {
        public Action<string> Log { private get; set; } = s => { };

        public void Inject(int pid, string path, string typeName, string methodName, string? parameter, bool x86)
        {
            IntPtr hProc = Native.OpenProcess(Native.ProcessAccessFlags.AllForDllInject, false, pid);
            if (hProc == IntPtr.Zero)
                throw new Exception("Couldn't open process");

            Log("Handle: " + hProc.ToInt32().ToString("X8"));

            var module = Process.GetProcessById(pid).Modules
                .OfType<ProcessModule>()
                .FirstOrDefault(m => m.ModuleName.Equals("mono.dll", StringComparison.OrdinalIgnoreCase))    // TODO: is there another type of dll?
                ?? throw new Exception("Could not find mono.dll");
            var exports = CodeInjectionUtils.GetAllExportAddresses(hProc, module.BaseAddress, x86);    // TODO: maybe don't return 800+ functions
            Log($"Got {exports.Count} exports in module {module.ModuleName}.");

            // Call remote functions to do actual injection
            var rootDomain = call("mono_get_root_domain");
            MsgBox.Instance.Show($"rootDomain: 0x{rootDomain.ToInt64():X}");
            // TODO: why is this 0?

            // var pStr = allocString(path);
            // var imageData = call("mono_pe_file_open", pStr);
            // MsgBox.Instance.Show($"imageData: 0x{imageData.ToInt64():X}");

            return;

            IntPtr getFunction(string fun) => exports.ContainsKey(fun)
                ? module.BaseAddress + exports[fun]
                : throw new Exception($"Could not find exported function {fun} in {module.ModuleName}");
            IntPtr call(string method, params object[] arguments) => CallFunction(hProc, getFunction(method), arguments, x86);

            IntPtr alloc(int size, int protection = 0x04) => Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size, 0x1000, protection);
            void writeBytes(IntPtr address, byte[] b) => Native.WriteProcessMemory(hProc, address, b, (uint)b.Length, out _);
            void writeString(IntPtr address, string str) => writeBytes(address, new ASCIIEncoding().GetBytes(str));

            IntPtr allocString(string? str)
            {
                if (str is null) return IntPtr.Zero;

                IntPtr pString = alloc(str.Length + 1);
                writeString(pString, str);
                return pString;
            }

            IntPtr allocBytes(byte[] buffer)
            {
                IntPtr pBuffer = alloc(buffer.Length);
                writeBytes(pBuffer, buffer);
                return pBuffer;
            }
        }

        private IntPtr CallFunction(IntPtr hProc, IntPtr fnAddr, object[] arguments, bool x86)
        {
            IntPtr pReturnValue = alloc(IntPtr.Size);

            var instructions = new InstructionList();

            void addCall(Register r, object[] callArgs)
            {
                foreach (Instruction instr in CodeInjectionUtils.CreateCallStub(r, callArgs, x86))
                    instructions.Add(instr);
            }

            if (x86) {
                // call CorBindToRuntimeEx
                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, fnAddr.ToInt32()));
                addCall(Register.EAX, arguments);
                instructions.Add(Instruction.Create(Code.Mov_rm32_r32, new MemoryOperand(Register.None, pReturnValue.ToInt32()), Register.EAX));

                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                throw new NotImplementedException();
            }

            Log("Instructions to be injected:\n" + string.Join("\n", instructions));

            var cw = new CodeWriterImpl();
            var ib = new InstructionBlock(cw, instructions, 0);
            bool success = BlockEncoder.TryEncode(x86 ? 32 : 64, ib, out string errMsg);
            if (!success)
                throw new Exception("Error during Iced encode: " + errMsg);
            byte[] bytes = cw.ToArray();

            var ptrStub = alloc(bytes.Length, 0x40);    // RWX
            writeBytes(ptrStub, bytes);

            var thread = Native.CreateRemoteThread(hProc, IntPtr.Zero, 0u, ptrStub, IntPtr.Zero, 0u, IntPtr.Zero);

            var err = Marshal.GetLastWin32Error();
            
            var outBuffer = new byte[IntPtr.Size];
            Native.ReadProcessMemory(hProc, pReturnValue, outBuffer, outBuffer.Length, out _);

            return new IntPtr(IntPtr.Size == 8 ? BitConverter.ToInt64(outBuffer, 0) : BitConverter.ToInt32(outBuffer, 0));

            IntPtr alloc(int size, int protection = 0x04) => Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size, 0x1000, protection);
            void writeBytes(IntPtr address, byte[] b) => Native.WriteProcessMemory(hProc, address, b, (uint)b.Length, out _);
        }

        sealed class CodeWriterImpl : CodeWriter {
            readonly List<byte> allBytes = new List<byte>();
            public override void WriteByte(byte value) => allBytes.Add(value);
            public byte[] ToArray() => allBytes.ToArray();
        }
    }
}
