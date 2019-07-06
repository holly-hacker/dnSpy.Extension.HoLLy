using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnSpy.Contracts.Debugger;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection
{
    [Export(typeof(ManagedInjector))]
    internal class ManagedInjector
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private readonly Lazy<DbgManager> dbgManagerLazy;

        [ImportingConstructor]
        public ManagedInjector(Lazy<DbgManager> dbgManagerLazy)
        {
            this.dbgManagerLazy = dbgManagerLazy;
        }

        public void Inject(int pid, MethodDef method, string parameter, bool x86, RuntimeType runtimeType)
            => Inject(pid, method.Module.Location, method.DeclaringType.FullName, method.Name, parameter, x86, runtimeType);

        public void Inject(int pid, string path, string typeName, string methodName, string parameter, bool x86, RuntimeType runtimeType)
        {
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

        private void InjectFramework(int pid, string path, string typeName, string methodName, string parameter, bool x86, bool isV4)
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

        public static bool IsProcessSupported(DbgProcess process, out string reason)
        {
            if (process is null) {
                reason = "no process found";
                return false;
            }

            if (process.OperatingSystem != DbgOperatingSystem.Windows) {
                reason = "Windows only";
                return false;
            }

            if (process.Architecture != DbgArchitecture.X86) {
                reason = "x86 only";
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

        private static IntPtr AllocateStub(IntPtr hProc, string asmPath, string typeName, string methodName, string args, IntPtr fnAddr, bool x86, bool isV4)
        {
            const string clrVersion2 = "v2.0.50727";
            const string clrVersion4 = "v4.0.30319";
            string clrVersion = isV4 ? clrVersion4 : clrVersion2;

            const string buildFlavor = "wks";    // WorkStation

            var clsidCLRRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
            var  iidICLRRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);

            IntPtr clrHost = alloc(IntPtr.Size);
            IntPtr riid = alloc(16);    // sizeof(GUID). originally 64, not sure why
            IntPtr rclsid = alloc(16);
            IntPtr buildFlavorPtr = alloc(buildFlavor.Length * 2 + 2);
            IntPtr clrVersionPtr = alloc(clrVersion.Length * 2 + 2);
            writeBytes(riid, iidICLRRuntimeHost.ToByteArray());
            writeBytes(rclsid, clsidCLRRuntimeHost.ToByteArray());
            writeString(buildFlavorPtr, buildFlavor);
            writeString(clrVersionPtr, clrVersion);

            IntPtr ptrRet = alloc(4);
            IntPtr ptrArgs = alloc(args.Length * 2 + 2);
            IntPtr ptrMethodName = alloc(methodName.Length * 2 + 2);
            IntPtr ptrTypeName = alloc(typeName.Length * 2 + 2);
            IntPtr ptrAsmPath = alloc(asmPath.Length * 2 + 2);
            writeString(ptrArgs, args);
            writeString(ptrMethodName, methodName);
            writeString(ptrTypeName, typeName);
            writeString(ptrAsmPath, asmPath);

            var instructions = new InstructionList();

            if (x86) {
                // call CorBindtoRuntimeEx
                instructions.Add(Instruction.Create(Code.Pushd_imm32, clrHost.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, riid.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, rclsid.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm8,  0));    // startupFlags
                instructions.Add(Instruction.Create(Code.Pushd_imm32, buildFlavorPtr.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, clrVersionPtr.ToInt32()));
                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, fnAddr.ToInt32()));
                instructions.Add(Instruction.Create(Code.Call_rm32, Register.EAX));

                // call pClrHost->Start();
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.None, clrHost.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.ECX, new MemoryOperand(Register.EAX)));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EDX, new MemoryOperand(Register.ECX, 0x0C)));
                instructions.Add(Instruction.Create(Code.Push_r32, Register.EAX));
                instructions.Add(Instruction.Create(Code.Call_rm32, Register.EDX));

                // call pClrHost->ExecuteInDefaultAppDomain()
                instructions.Add(Instruction.Create(Code.Pushd_imm32, ptrRet.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, ptrArgs.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, ptrMethodName.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, ptrTypeName.ToInt32()));
                instructions.Add(Instruction.Create(Code.Pushd_imm32, ptrAsmPath.ToInt32()));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.None, clrHost.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.ECX, new MemoryOperand(Register.EAX)));
                instructions.Add(Instruction.Create(Code.Push_r32, Register.EAX));
                instructions.Add(Instruction.Create(Code.Mov_r32_rm32, Register.EAX, new MemoryOperand(Register.ECX, 0x2C)));
                instructions.Add(Instruction.Create(Code.Call_rm32, Register.EAX));

                instructions.Add(Instruction.Create(Code.Retnd));
            } else {
                int lowerHalf(IntPtr ptr) => (int)(ptr.ToInt64() & 0xFFFFFFFF);
                int upperHalf(IntPtr ptr) => (int)(ptr.ToInt64() >> 32);

                // call CorBindtoRuntimeEx
                // https://docs.microsoft.com/en-us/cpp/build/x64-calling-convention?view=vs-2019
                // TODO: crashes at first Call
                // TODO: why does Pushq_imm32 push 64 bits??
                // TODO: still using 32bit pointers in places
                // TODO: only up to first call is tested
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(clrHost)));    // rbp+30h
                // instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(clrHost)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(riid)));       // rbp+28h
                // instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(riid)));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.R9, rclsid.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.R8D,  0));    // startupFlags, perhaps 1 for concurrent gc?
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RDX, buildFlavorPtr.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RCX, clrVersionPtr.ToInt64()));
                instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RAX, fnAddr.ToInt64()));
                for (int i = 0; i < 8/2; i++) instructions.Add(Instruction.Create(Code.Pushq_imm32, 0));    // push shadow space because x64
                instructions.Add(Instruction.Create(Code.Call_rm64, Register.RAX));    // this crashes

                // call pClrHost->Start();
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.None, clrHost.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RCX, new MemoryOperand(Register.RAX)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RDX, new MemoryOperand(Register.RCX, 0x0C)));
                instructions.Add(Instruction.Create(Code.Push_r64, Register.RAX));
                instructions.Add(Instruction.Create(Code.Call_rm64, Register.RDX));

                // call pClrHost->ExecuteInDefaultAppDomain()
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(ptrRet)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(ptrRet)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(ptrArgs)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(ptrArgs)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(ptrMethodName)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(ptrMethodName)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(ptrTypeName)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(ptrTypeName)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, lowerHalf(ptrAsmPath)));
                instructions.Add(Instruction.Create(Code.Pushq_imm32, upperHalf(ptrAsmPath)));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.None, clrHost.ToInt32())));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RCX, new MemoryOperand(Register.RAX)));
                instructions.Add(Instruction.Create(Code.Push_r64, Register.RAX));
                instructions.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RCX, 0x2C)));
                instructions.Add(Instruction.Create(Code.Call_rm64, Register.RAX));

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
        }

        sealed class CodeWriterImpl : CodeWriter {
            readonly List<byte> allBytes = new List<byte>();
            public override void WriteByte(byte value) => allBytes.Add(value);
            public byte[] ToArray() => allBytes.ToArray();
        }
    }
}
