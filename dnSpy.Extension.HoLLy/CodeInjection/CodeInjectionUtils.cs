using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection
{
    internal static class CodeInjectionUtils
    {
	    public static int GetExportAddress(IntPtr hProc, IntPtr hMod, string name, bool x86)
	    {
		    var dic = GetAllExportAddresses(hProc, hMod, x86);

		    if (!dic.ContainsKey(name))
			    throw new Exception($"Could not find function with name {name}.");

		    return dic[name];
	    }

        public static Dictionary<string, int> GetAllExportAddresses(IntPtr hProc, IntPtr hMod, bool x86)
		{
			var dic = new Dictionary<string, int>();
			int hdr = readInt(0x3C);

			int exportTableRva = readInt(hdr + (x86 ? 0x78 : 0x88));
			var exportTable = readStruct<ImageExportDirectory>(exportTableRva);

			int[] functions = readArray<int>(exportTable.AddressOfFunctions, exportTable.NumberOfFunctions);
			int[] names = readArray<int>(exportTable.AddressOfNames, exportTable.NumberOfNames);
			ushort[] ordinals = readArray<ushort>(exportTable.AddressOfNameOrdinals, exportTable.NumberOfFunctions);

			for (int i = 0; i < names.Length; i++)
				if (names[i] != 0)
					dic[readCString(names[i])] = functions[ordinals[i]];

			return dic;

			#region local memory reading functions
			byte[] readBytes(int offset, int size)
			{
				var readBuffer = new byte[size];

				if (!Native.ReadProcessMemory(hProc, hMod + offset, readBuffer, size, out _))
					throw new Exception($"Reading at address 0x{(hMod + offset).ToInt64():X8} failed");

				return readBuffer;
			}

			int readInt(int offset) => BitConverter.ToInt32(readBytes(offset, 4), 0);

			T[] readArray<T>(uint offset, uint amount)
			{
				byte[] bytes = readBytes((int)offset, (int)(amount * Marshal.SizeOf<T>()));

				var arr = new T[amount];
				Buffer.BlockCopy(bytes, 0, arr, 0, bytes.Length);
				return arr;
			}

			T readStruct<T>(int offset)
			{
				byte[] bytes = readBytes(offset, Marshal.SizeOf<T>());

				IntPtr hStructure = Marshal.AllocHGlobal(bytes.Length);
				Marshal.Copy(bytes, 0, hStructure, bytes.Length);
				var structure = Marshal.PtrToStructure<T>(hStructure);
				Marshal.FreeHGlobal(hStructure);

				return structure;
			}

			string readCString(int offset)
			{
				byte b;
				var str = new StringBuilder();

				for (int i = 0; (b = readBytes(offset+i, 1)[0]) != 0; i++)
					str.Append((char)b);

				return str.ToString();
			}
			#endregion
		}

        public static void AddCallStub(InstructionList instructions, IntPtr regAddr, object[] arguments, bool x86, bool cleanStack = false)
        {
	        if (x86) {
		        instructions.Add(Instruction.Create(Code.Mov_r32_imm32, Register.EAX, regAddr.ToInt32()));
		        AddCallStub(instructions, Register.EAX, arguments, true, cleanStack);
	        } else {
		        instructions.Add(Instruction.Create(Code.Mov_r64_imm64, Register.RAX, regAddr.ToInt64()));
		        AddCallStub(instructions, Register.RAX, arguments, false, cleanStack);
	        }
        }

        public static void AddCallStub(InstructionList instructions, Register regFun, object[] arguments, bool x86, bool cleanStack = false)
        {
	        // TODO: push/pop parameter registers?
	        if (x86) {
		        // push arguments
		        for (int i = arguments.Length - 1; i >= 0; i--) {
			        instructions.Add(arguments[i] switch {
				        IntPtr p => Instruction.Create(Code.Pushd_imm32, p.ToInt32()),
				        int i32 => Instruction.Create(Code.Pushd_imm32, i32),
				        byte u8 => Instruction.Create(Code.Pushd_imm8, u8),
				        Register reg => Instruction.Create(Code.Push_r32, reg),
				        _ => throw new NotSupportedException($"Unsupported parameter type {arguments[i].GetType()} on x86"),
			        });
		        }

		        instructions.Add(Instruction.Create(Code.Call_rm32, regFun));

		        if (cleanStack && arguments.Length > 0)
			        instructions.Add(Instruction.Create(Code.Add_rm32_imm8, Register.ESP, arguments.Length * IntPtr.Size));
	        } else {
		        // calling convention: https://docs.microsoft.com/en-us/cpp/build/x64-calling-convention?view=vs-2019
		        const Register tempReg = Register.RAX;

		        // push the temp register so we can use it
		        instructions.Add(Instruction.Create(Code.Push_r64, tempReg));

		        // set arguments
		        for (int i = arguments.Length - 1; i >= 0; i--) {
			        var arg = arguments[i];
			        Register argReg = i switch { 0 => Register.RCX, 1 => Register.RDX, 2 => Register.R8, 3 => Register.R9, _ => Register.None };
			        if (i > 3) {
				        // push on the stack, keeping in mind that we pushed the temp reg onto the stack too
				        if (arg is Register r) {
					        instructions.Add(Instruction.Create(Code.Mov_rm64_r64, new MemoryOperand(Register.RSP, 0x20 + (i - 3) * 8), r));
				        } else {
					        instructions.Add(Instruction.Create(Code.Mov_r64_imm64, tempReg, convertToLong(arg)));
					        instructions.Add(Instruction.Create(Code.Mov_rm64_r64, new MemoryOperand(Register.RSP, 0x20 + (i - 3) * 8), tempReg));
				        }
			        } else {
				        // move to correct register
				        if (arg is Register r) {
					        instructions.Add(Instruction.Create(Code.Mov_r64_rm64, argReg, r));
				        } else {
							instructions.Add(Instruction.Create(Code.Mov_r64_imm64, argReg, convertToLong(arg)));
				        }
			        }

			        long convertToLong(object o) => o switch {
				        IntPtr p => p.ToInt64(),
				        UIntPtr p => (long)p.ToUInt64(),
				        _ => Convert.ToInt64(o),
			        };
		        }

		        // pop temp register again
		        instructions.Add(Instruction.Create(Code.Pop_r64, tempReg));

		        // call the function
		        instructions.Add(Instruction.Create(Code.Call_rm64, regFun));
	        }
        }

        public static IntPtr RunRemoteCode(IntPtr hProc, InstructionList instructions, bool x86, bool waitForFinish = false)
        {
	        var cw = new CodeWriterImpl();
	        var ib = new InstructionBlock(cw, instructions, 0);
	        if (!BlockEncoder.TryEncode(x86 ? 32 : 64, ib, out string errMsg))
		        throw new Exception("Error during Iced encode: " + errMsg);
	        byte[] bytes = cw.ToArray();

	        var ptrStub = Native.VirtualAllocEx(hProc, IntPtr.Zero, (uint)bytes.Length, 0x1000, 0x40);
	        Native.WriteProcessMemory(hProc, ptrStub, bytes, (uint)bytes.Length, out _);

	        var thread = Native.CreateRemoteThread(hProc, IntPtr.Zero, 0u, ptrStub, IntPtr.Zero, 0u, IntPtr.Zero);

	        // wait for thread to finish
	        if (waitForFinish)
				Native.WaitForSingleObject(thread, uint.MaxValue);

	        return thread;
        }

        private sealed class CodeWriterImpl : CodeWriter {
	        readonly List<byte> allBytes = new List<byte>();
	        public override void WriteByte(byte value) => allBytes.Add(value);
	        public byte[] ToArray() => allBytes.ToArray();
        }

        private struct ImageExportDirectory
        {
#pragma warning disable 649
	        public uint Characteristics;
	        public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;

            public uint Name;
            public uint Base;
            public uint NumberOfFunctions;
            public uint NumberOfNames;
            public uint AddressOfFunctions;
            public uint AddressOfNames;
            public uint AddressOfNameOrdinals;
#pragma warning restore 649
        }
    }
}
