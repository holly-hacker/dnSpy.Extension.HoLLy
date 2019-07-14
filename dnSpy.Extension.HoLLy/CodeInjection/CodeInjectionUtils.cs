using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
