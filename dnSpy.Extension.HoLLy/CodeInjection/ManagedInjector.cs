using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dnSpy.Contracts.Debugger;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection
{
    [Export(typeof(ManagedInjector))]
    internal class ManagedInjector
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private readonly Lazy<DbgManager> dbgManagerLazy;
        private readonly Settings.Settings settings;

        [ImportingConstructor]
        public ManagedInjector(Lazy<DbgManager> dbgManagerLazy, Settings.Settings settings)
        {
            this.dbgManagerLazy = dbgManagerLazy;
            this.settings = settings;
        }

        public void Inject(int pid, InjectionArguments args, bool x86, RuntimeType runtimeType)
            => Inject(pid, args.Path, args.Type, args.Method, args.Argument, x86, runtimeType);

        public void Inject(int pid, string path, string typeName, string methodName, string? parameter, bool x86, RuntimeType runtimeType)
        {
            if (settings.CopyInjectedDLLToTemp)
                path = Utils.CopyToTempPath(path);

            switch (runtimeType) {
                case RuntimeType.FrameworkV2:
                case RuntimeType.FrameworkV4:
                    InjectFramework(pid, path, typeName, methodName, parameter, x86, runtimeType == RuntimeType.FrameworkV4);
                    break;
                case RuntimeType.NetCore:
                case RuntimeType.Unity:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(runtimeType), runtimeType, null);
            }

        }

        private void InjectFramework(int pid, string path, string typeName, string methodName, string? parameter, bool x86, bool isV4)
        {
            IntPtr hProc = Native.OpenProcess(Native.ProcessAccessFlags.AllForDllInject, false, pid);
            if (hProc == IntPtr.Zero)
                throw new Exception("Couldn't open process");

            DbgManager.WriteMessage("Handle: " + hProc.ToInt32().ToString("X8"));

            var bindToRuntimeAddr = GetCorBindToRuntimeExAddress(pid, hProc, x86);
            DbgManager.WriteMessage("CurBindToRuntimeEx: " + bindToRuntimeAddr.ToInt64().ToString("X8"));

            var hStub = AllocateStub(hProc, path, typeName, methodName, parameter, bindToRuntimeAddr, x86, isV4);
            DbgManager.WriteMessage("Created stub at: " + hStub.ToInt64().ToString("X8"));

            var hThread = Native.CreateRemoteThread(hProc, IntPtr.Zero, 0u, hStub, IntPtr.Zero, 0u, IntPtr.Zero);
            DbgManager.WriteMessage("Thread handle: " + hThread.ToInt32().ToString("X8"));

            var success = Native.GetExitCodeThread(hThread, out IntPtr exitCode);
            DbgManager.WriteMessage("GetExitCode success: " + success);
            DbgManager.WriteMessage("Exit code: " + exitCode.ToInt32().ToString("X8"));

            Native.CloseHandle(hProc);
        }

        public static bool IsProcessSupported(DbgProcess process, out string? reason)
        {
            if (process is null) {
                reason = "no process found";
                return false;
            }

            if (process.OperatingSystem != DbgOperatingSystem.Windows) {
                reason = "Windows only";
                return false;
            }

            if (process.Architecture != DbgArchitecture.X86 && process.Architecture != DbgArchitecture.X64) {
                reason = "x86 and x64 only";
                return false;
            }

            var runtimeType = process.Runtimes.First().GetRuntimeType();
            var rtSupported = runtimeType switch {
                RuntimeType.FrameworkV2 => true,
                RuntimeType.FrameworkV4 => true,
                _ => false,
            };

            if (!rtSupported) {
                reason = $"Unsupported runtime '{process.Runtimes.First().Name}'";
                return false;
            }

            reason = null;
            return true;
        }

        private static IntPtr GetCorBindToRuntimeExAddress(int pid, IntPtr hProc, bool x86)
        {
            var proc = Process.GetProcessById(pid);
            var mod = proc.Modules.OfType<ProcessModule>().FirstOrDefault(m => m.ModuleName.Equals("mscoree.dll", StringComparison.InvariantCultureIgnoreCase));

            if (mod is null)
                throw new Exception("Couldn't find MSCOREE.DLL, arch mismatch?");

            int fnAddr = PE.GetExportAddress(hProc, mod.BaseAddress, "CorBindToRuntimeEx", x86);

            return mod.BaseAddress + fnAddr;
        }

        private static IntPtr AllocateStub(IntPtr hProc, string asmPath, string typeName, string methodName, string? args, IntPtr fnAddr, bool x86, bool isV4)
        {
            const string clrVersion2 = "v2.0.50727";
            const string clrVersion4 = "v4.0.30319";
            string clrVersion = isV4 ? clrVersion4 : clrVersion2;

            const string buildFlavor = "wks";    // WorkStation

            var clsidCLRRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
            var  iidICLRRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);

            IntPtr ppv = alloc(IntPtr.Size);
            IntPtr riid = allocBytes(iidICLRRuntimeHost.ToByteArray());
            IntPtr rcslid = allocBytes(clsidCLRRuntimeHost.ToByteArray());
            IntPtr pwszBuildFlavor = allocString(buildFlavor);
            IntPtr pwszVersion = allocString(clrVersion);

            IntPtr pReturnValue = alloc(4);
            IntPtr pwzArgument = allocString(args);
            IntPtr pwzMethodName = allocString(methodName);
            IntPtr pwzTypeName = allocString(typeName);
            IntPtr pwzAssemblyPath = allocString(asmPath);

            var instructions = new InstructionList();

            if (x86) {
                // call CorBindtoRuntimeEx
                instructions.Add(Instruction.Create(Code.Pushd_imm32, ppv.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, riid.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, rcslid.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm8,  0));    // startupFlags
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pwszBuildFlavor.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pwszVersion.ToInt32()));
                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, fnAddr.ToInt32()));
                instructions.Add(Instruction.Create(Code.Call_rm32, Register.EAX));

                // call ICLRRuntimeHost::Start
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.None, ppv.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.ECX, new MemoryOperand(Register.EAX)));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EDX, new MemoryOperand(Register.ECX, 0x0C)));
                instructions.Add(Instruction.Create(Code.Push_r32, Register.EAX));
                instructions.Add(Instruction.Create(Code.Call_rm32, Register.EDX));

                // call ICLRRuntimeHost::ExecuteInDefaultAppDomain
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pReturnValue.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pwzArgument.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pwzMethodName.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pwzTypeName.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, pwzAssemblyPath.ToInt32()));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.None, ppv.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.ECX, new MemoryOperand(Register.EAX)));
                instructions.Add(Instruction.Create(Code.Push_r32, Register.EAX));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.ECX, 0x2C)));
                instructions.Add(Instruction.Create(Code.Call_rm32, Register.EAX));

                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                const int maxStackIndex = 3;
                const int stackSize = 0x20 + maxStackIndex*8;    // 0x20 bytes shadow space, because x64
                MemoryOperand stackAccess(int i) => new MemoryOperand(Register.RSP, stackSize - (maxStackIndex - i) * 8);

                instructions.Add(Instruction.Create(Code.Sub_rm64_imm8, Register.RSP, stackSize));

                // call CorBindtoRuntimeEx
                // calling convention: https://docs.microsoft.com/en-us/cpp/build/x64-calling-convention?view=vs-2019
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RAX, ppv.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_rm64_r64, stackAccess(1), Register.RAX));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RAX, riid.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_rm64_r64, stackAccess(0), Register.RAX));

                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.R9, rcslid.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.R8D, 0));    // startupFlags
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RDX, pwszBuildFlavor.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RCX, pwszVersion.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RAX, fnAddr.ToInt64()));
                instructions.Add(Instruction.Create(Code.Call_rm64, Register.RAX));

                // call pClrHost->Start();
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RCX, ppv.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RCX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RDX, new MemoryOperand(Register.RAX, 0x18)));
                instructions.Add(Instruction.Create(Code.Call_rm64, Register.RDX));

                // call pClrHost->ExecuteInDefaultAppDomain()
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RDX, pReturnValue.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_rm64_r64, stackAccess(1), Register.RDX));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RDX, pwzArgument.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_rm64_r64, stackAccess(0), Register.RDX));

                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.R9, pwzMethodName.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.R8, pwzTypeName.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RDX, pwzAssemblyPath.ToInt64()));

                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RCX, ppv.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RCX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RCX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RAX, 0x58)));
                instructions.Add(Instruction.Create(Code.Call_rm64, Register.RAX));

                instructions.Add(Instruction.Create(Code.Add_rm64_imm8, Register.RSP, stackSize));

                instructions.Add(Instruction.Create(Code.Retnq));
            }

            var cw = new CodeWriterImpl();
            var ib = new InstructionBlock(cw, instructions, 0);
            bool success = BlockEncoder.TryEncode(x86 ? 32 : 64, ib, out string errMsg);
            if (!success)
                throw new Exception("Error during Iced encode: " + errMsg);
            byte[] bytes = cw.ToArray();

            var ptrStub = alloc(bytes.Length, 0x40);    // RWX
            writeBytes(ptrStub, bytes);

            return ptrStub;

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

        sealed class CodeWriterImpl : CodeWriter {
            readonly List<byte> allBytes = new List<byte>();
            public override void WriteByte(byte value) => allBytes.Add(value);
            public byte[] ToArray() => allBytes.ToArray();
        }
    }
}
