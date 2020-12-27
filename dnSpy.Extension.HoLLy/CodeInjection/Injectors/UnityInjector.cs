using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HoLLy.dnSpyExtension.Common.CodeInjection;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    internal class UnityInjector : IInjector
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
                .FirstOrDefault(m => m.ModuleName?.Equals("mono.dll", StringComparison.OrdinalIgnoreCase) == true || m.ModuleName?.Equals("mono-2.0-bdwgc.dll", StringComparison.OrdinalIgnoreCase) == true)
                ?? throw new Exception("Could not find Mono module in process");
            var exports = CodeInjectionUtils.GetAllExportAddresses(hProc, module.BaseAddress, x86);    // TODO: maybe don't return 800+ functions
            Log($"Got {exports.Count} exports in module {module.ModuleName}.");

            // Call remote functions to do actual injection
            const int amountOfParameters = 1;
            (IntPtr, IntPtr)? rootDomainInfo = null;
            IntPtr rootDomain = call("mono_get_root_domain");
            var rawImage = call("mono_image_open", allocCString(args.Path), IntPtr.Zero);

            // from now on, run mono_thread_attach every time
            rootDomainInfo = (getFunction("mono_thread_attach"), rootDomain);

            var assembly = call("mono_assembly_load_from_full", rawImage, allocCString(""), IntPtr.Zero, 0);
            var image = call("mono_assembly_get_image", assembly);
            // TODO: what if null namespace?
            var @class = call("mono_class_from_name", image, allocCString(args.Namespace), allocCString(args.Type));
            var method = call("mono_class_get_method_from_name", @class, allocCString(args.Method), amountOfParameters);

            // allocate arguments
            var pArg1 = call("mono_string_new_wrapper", allocCString(args.Argument));
            var pArgs = allocBytes(x86 ? BitConverter.GetBytes(pArg1.ToInt32()) : BitConverter.GetBytes(pArg1.ToInt64()));
            callNoWait("mono_runtime_invoke", method, IntPtr.Zero, pArgs, IntPtr.Zero);

            return;

            IntPtr getFunction(string fun) => exports.ContainsKey(fun)
                ? module.BaseAddress + exports[fun]
                : throw new Exception($"Could not find exported function {fun} in {module.ModuleName}");

            IntPtr call(string m, params object[] arguments)
            {
                var ret = CallFunction(hProc, getFunction(m), arguments, x86, rootDomainInfo);
                Log($"{m} returned 0x{ret.ToInt64():X}");
                return ret;
            }

            void callNoWait(string m, params object[] arguments)
            {
                CallFunction(hProc, getFunction(m), arguments, x86, (getFunction("mono_thread_attach"), rootDomain), false);
                Log($"Executed {m}");
            }

            IntPtr allocCString(string? str) => str is null ? allocBytes(new byte[] { 0 }) : allocBytes(new ASCIIEncoding().GetBytes(str + "\0"));

            IntPtr allocBytes(byte[] buffer)
            {
                IntPtr pBuffer = Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)buffer!.Length, 0x1000, 0x04);
                if (buffer.Any(b => b != 0))
                    Native.WriteProcessMemory(hProc, pBuffer, buffer, (uint)buffer.Length, out _);
                return pBuffer;
            }
        }

        private static IntPtr CallFunction(IntPtr hProc, IntPtr fnAddr, object[] arguments, bool x86, (IntPtr fun, IntPtr addr)? rootDomainInfo = null, bool wait = true)
        {
            IntPtr pReturnValue = alloc(IntPtr.Size);

            var instructions = new InstructionList();

            // mono uses the cdecl calling convention, so clean the stack (x86 only)
            void addCall(IntPtr fn, object[] callArgs) => CodeInjectionUtils.AddCallStub(instructions, fn, callArgs, x86, x86);

            if (x86) {
                if (rootDomainInfo.HasValue)
                    addCall(rootDomainInfo.Value.fun, new object[] { rootDomainInfo.Value.addr });

                addCall(fnAddr, arguments);
                instructions.Add(Instruction.Create(Code.Mov_rm32_r32, new MemoryOperand(Register.None, pReturnValue.ToInt32()), Register.EAX));
                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                int stackSize = 0x20 + Math.Max(0, arguments.Length - 4) * 8;
                instructions.Add(Instruction.Create(Code.Sub_rm64_imm8, Register.RSP, stackSize));

                if (rootDomainInfo.HasValue)
                    addCall(rootDomainInfo.Value.fun, new object[] { rootDomainInfo.Value.addr });

                addCall(fnAddr, arguments);
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RBX, pReturnValue.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_rm64_r64, new MemoryOperand(Register.RBX), Register.RAX));

                instructions.Add(Instruction.Create(Code.Add_rm64_imm8, Register.RSP, stackSize));
                instructions.Add(Instruction.Create(Code.Retnq));
            }

            CodeInjectionUtils.RunRemoteCode(hProc, instructions, x86, wait);

            if (!wait)
                return IntPtr.Zero;

            var outBuffer = new byte[IntPtr.Size];
            Native.ReadProcessMemory(hProc, pReturnValue, outBuffer, outBuffer.Length, out _);

            return new IntPtr(IntPtr.Size == 8 ? BitConverter.ToInt64(outBuffer, 0) : BitConverter.ToInt32(outBuffer, 0));

            IntPtr alloc(int size, int protection = 0x04) => Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size, 0x1000, protection);
        }
    }
}
