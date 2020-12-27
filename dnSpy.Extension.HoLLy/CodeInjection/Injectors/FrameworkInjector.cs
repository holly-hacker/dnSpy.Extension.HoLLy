using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HoLLy.dnSpyExtension.Common.CodeInjection;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    internal class FrameworkV2Injector : FrameworkInjector
    {
        protected override string ClrVersion => "v2.0.50727";
    }

    internal class FrameworkV4Injector : FrameworkInjector
    {
        protected override string ClrVersion => "v4.0.30319";
    }

    internal abstract class FrameworkInjector : IInjector
    {
        protected abstract string ClrVersion { get; }
        public Action<string> Log { private get; set; } = _ => { };

        public void Inject(int pid, in InjectionArguments args, bool x86)
        {
            IntPtr hProc = Native.OpenProcess(Native.ProcessAccessFlags.AllForDllInject, false, pid);
            if (hProc == IntPtr.Zero)
                throw new Exception("Couldn't open process");

            Log("Handle: " + hProc.ToInt32().ToString("X8"));

            var bindToRuntimeAddr = GetCorBindToRuntimeExAddress(pid, hProc, x86);
            Log("CurBindToRuntimeEx: " + bindToRuntimeAddr.ToInt64().ToString("X8"));

            var instructions = CreateStub(hProc, args.Path, args.TypeFull, args.Method, args.Argument, bindToRuntimeAddr, x86, ClrVersion);
            Log("Instructions to be injected:\n" + string.Join("\n", instructions));

            var hThread = CodeInjectionUtils.RunRemoteCode(hProc, instructions, x86);
            Log("Thread handle: " + hThread.ToInt32().ToString("X8"));

            // TODO: option to wait until injected function returns?
            /*
            var success = Native.GetExitCodeThread(hThread, out IntPtr exitCode);
            Log("GetExitCode success: " + success);
            Log("Exit code: " + exitCode.ToInt32().ToString("X8"));
            */

            Native.CloseHandle(hProc);
        }

        private static IntPtr GetCorBindToRuntimeExAddress(int pid, IntPtr hProc, bool x86)
        {
            var proc = Process.GetProcessById(pid);
            var mod = proc.Modules.OfType<ProcessModule>().FirstOrDefault(m => m.ModuleName?.Equals("mscoree.dll", StringComparison.InvariantCultureIgnoreCase) == true);

            if (mod is null)
                throw new Exception("Couldn't find MSCOREE.DLL, arch mismatch?");

            int fnAddr = CodeInjectionUtils.GetExportAddress(hProc, mod.BaseAddress, "CorBindToRuntimeEx", x86);

            return mod.BaseAddress + fnAddr;
        }

        private InstructionList CreateStub(IntPtr hProc, string asmPath, string typeFullName, string methodName, string? args, IntPtr fnAddr, bool x86, string clrVersion)
        {
            const string buildFlavor = "wks";    // WorkStation

            var clsidClrRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
            var  iidIclrRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);

            IntPtr ppv = alloc(IntPtr.Size);
            IntPtr riid = allocBytes(iidIclrRuntimeHost.ToByteArray());
            IntPtr rcslid = allocBytes(clsidClrRuntimeHost.ToByteArray());
            IntPtr pwszBuildFlavor = allocString(buildFlavor);
            IntPtr pwszVersion = allocString(clrVersion);

            IntPtr pReturnValue = alloc(4);
            IntPtr pwzArgument = allocString(args);
            IntPtr pwzMethodName = allocString(methodName);
            IntPtr pwzTypeName = allocString(typeFullName);
            IntPtr pwzAssemblyPath = allocString(asmPath);

            var instructions = new InstructionList();

            void addCallR(Register r, params object[] callArgs) => CodeInjectionUtils.AddCallStub(instructions, r, callArgs, x86);
            void addCallP(IntPtr fn, params object[] callArgs) => CodeInjectionUtils.AddCallStub(instructions, fn, callArgs, x86);

            if (x86) {
                // call CorBindToRuntimeEx
                addCallP(fnAddr, pwszVersion, pwszBuildFlavor, (byte)0, rcslid, riid, ppv);

                // call ICLRRuntimeHost::Start
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EDX, new MemoryOperand(Register.None, ppv.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.EDX)));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.EAX, 0x0C)));
                addCallR(Register.EAX, Register.EDX);

                // call ICLRRuntimeHost::ExecuteInDefaultAppDomain
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EDX, new MemoryOperand(Register.None, ppv.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.EDX)));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.EAX, 0x2C)));
                addCallR(Register.EAX, Register.EDX, pwzAssemblyPath, pwzTypeName, pwzMethodName, pwzArgument, pReturnValue);

                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                const int maxStackIndex = 3;
                const int stackOffset = 0x20;
                instructions.Add(Instruction.Create(Code.Sub_rm64_imm8, Register.RSP, stackOffset + maxStackIndex * 8));

                // call CorBindToRuntimeEx
                addCallP(fnAddr, pwszVersion, pwszBuildFlavor, 0, rcslid, riid, ppv);

                // call pClrHost->Start();
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RCX, ppv.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RCX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RDX, new MemoryOperand(Register.RAX, 0x18)));
                addCallR(Register.RDX, Register.RCX);

                // call pClrHost->ExecuteInDefaultAppDomain()
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RCX, ppv.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RCX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RAX, 0x58)));
                addCallR(Register.RAX, Register.RCX, pwzAssemblyPath, pwzTypeName, pwzMethodName, pwzArgument, pReturnValue);

                instructions.Add(Instruction.Create(Code.Add_rm64_imm8, Register.RSP, stackOffset + maxStackIndex * 8));

                instructions.Add(Instruction.Create(Code.Retnq));
            }

            return instructions;

            IntPtr alloc(int size, int protection = 0x04) => Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size, 0x1000, protection);
            void writeBytes(IntPtr address, byte[] b) => Native.WriteProcessMemory(hProc, address, b, (uint)b.Length, out _);
            void writeString(IntPtr address, string str) => writeBytes(address, new UnicodeEncoding().GetBytes(str));

            IntPtr allocString(string? str)
            {
                if (str is null) return IntPtr.Zero;

                IntPtr pString = alloc(str.Length * 2 + 2);
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
    }
}
