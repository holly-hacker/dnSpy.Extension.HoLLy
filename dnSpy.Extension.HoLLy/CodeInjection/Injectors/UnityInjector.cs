using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using dnSpy.Contracts.App;
using HoLLy.dnSpyExtension.Common.CodeInjection;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    public class UnityInjector : IInjector
    {
        public Action<string> Log { private get; set; } = s => { };

        public void Inject(int pid, in InjectionArguments args, bool x86)
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
            const int amountOfParameters = 1;
            var rootDomain = call("mono_get_root_domain");
            var rawImage = call("mono_image_open", allocCString(args.Path), IntPtr.Zero);
            var assembly = callThread("mono_assembly_load_from_full", rawImage, allocCString(""), IntPtr.Zero, 0);
            var image = callThread("mono_assembly_get_image", assembly);
            // TODO: what if null namespace?
            var @class = callThread("mono_class_from_name", image, allocCString(args.Namespace), allocCString(args.Type));
            var method = callThread("mono_class_get_method_from_name", @class, allocCString(args.Method), amountOfParameters);

            // allocate arguments
            var pArg1 = call("mono_string_new", rootDomain, allocCString(args.Argument));
            var pArgs = allocBytes(x86 ? BitConverter.GetBytes(pArg1.ToInt32()) : BitConverter.GetBytes(pArg1.ToInt64()));
            callThreadNoWait("mono_runtime_invoke", method, IntPtr.Zero, pArgs, IntPtr.Zero);

            return;

            IntPtr getFunction(string fun) => exports.ContainsKey(fun)
                ? module.BaseAddress + exports[fun]
                : throw new Exception($"Could not find exported function {fun} in {module.ModuleName}");

            IntPtr call(string method, params object[] arguments)
            {
                var ret = CallFunction(hProc, getFunction(method), arguments, x86);
                Log($"{method} returned 0x{ret.ToInt64():X}");
                return ret;
            }

            IntPtr callThread(string method, params object[] arguments)
            {
                var ret = CallFunction(hProc, getFunction(method), arguments, x86, (getFunction("mono_thread_attach"), rootDomain));
                Log($"{method} returned 0x{ret.ToInt64():X}");
                return ret;
            }

            void callThreadNoWait(string method, params object[] arguments)
            {
                CallFunctionNoWait(hProc, getFunction(method), arguments, x86, (getFunction("mono_thread_attach"), rootDomain));
                Log($"Executed {method}");
            }

            IntPtr alloc(int size, int protection = 0x04) => Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size, 0x1000, protection);
            void writeBytes(IntPtr address, byte[] b) => Native.WriteProcessMemory(hProc, address, b, (uint)b.Length, out _);

            IntPtr allocCString(string? str) => str is null ? IntPtr.Zero : allocBytes(new ASCIIEncoding().GetBytes(str + "\0"));
            IntPtr allocString(string? str) => str is null ? IntPtr.Zero : allocBytes(new UnicodeEncoding().GetBytes(str + "\0"));

            IntPtr allocBytes(byte[] buffer)
            {
                IntPtr pBuffer = alloc(buffer.Length);
                writeBytes(pBuffer, buffer);
                return pBuffer;
            }
        }

        private IntPtr CallFunction(IntPtr hProc, IntPtr fnAddr, object[] arguments, bool x86, (IntPtr fun, IntPtr addr)? rootDomainInfo = null)
        {
            IntPtr pReturnValue = alloc(IntPtr.Size);

            var instructions = new InstructionList();

            void addCall(Register r, object[] callArgs)
            {
                // cdecl calling conventions, so we need to clean the stack
                foreach (Instruction instr in CodeInjectionUtils.CreateCallStub(r, callArgs, x86, x86))
                    instructions.Add(instr);
            }

            if (x86) {
                if (rootDomainInfo.HasValue) {
                    instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, rootDomainInfo.Value.fun.ToInt32()));
                    addCall(Register.EAX, new object[] { rootDomainInfo.Value.addr });
                }

                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, fnAddr.ToInt32()));
                addCall(Register.EAX, arguments);
                instructions.Add(Instruction.Create(Code.Mov_rm32_r32, new MemoryOperand(Register.None, pReturnValue.ToInt32()), Register.EAX));

                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                throw new NotImplementedException();
            }

            // Log("Instructions to be injected:\n" + string.Join("\n", instructions));
            CodeInjectionUtils.RunRemoteCode(hProc, instructions, x86, true);

            var outBuffer = new byte[IntPtr.Size];
            Native.ReadProcessMemory(hProc, pReturnValue, outBuffer, outBuffer.Length, out _);

            return new IntPtr(IntPtr.Size == 8 ? BitConverter.ToInt64(outBuffer, 0) : BitConverter.ToInt32(outBuffer, 0));

            IntPtr alloc(int size, int protection = 0x04) => Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size, 0x1000, protection);
        }

        // TODO: duplicated method is very, very ugly! I want to refactor most of this too
        private void CallFunctionNoWait(IntPtr hProc, IntPtr fnAddr, object[] arguments, bool x86, (IntPtr fun, IntPtr addr)? rootDomainInfo = null)
        {
            var instructions = new InstructionList();

            void addCall(Register r, object[] callArgs)
            {
                // cdecl calling conventions, so we need to clean the stack
                foreach (Instruction instr in CodeInjectionUtils.CreateCallStub(r, callArgs, x86, x86))
                    instructions.Add(instr);
            }

            if (x86) {
                if (rootDomainInfo.HasValue) {
                    instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, rootDomainInfo.Value.fun.ToInt32()));
                    addCall(Register.EAX, new object[] { rootDomainInfo.Value.addr });
                }

                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, fnAddr.ToInt32()));
                addCall(Register.EAX, arguments);

                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                throw new NotImplementedException();
            }

            // Log("Instructions to be injected:\n" + string.Join("\n", instructions));
            CodeInjectionUtils.RunRemoteCode(hProc, instructions, x86);
        }
    }
}
