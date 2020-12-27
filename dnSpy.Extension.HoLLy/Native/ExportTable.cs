using System.Linq;
using System.Text;
using dnlib.IO;
using dnlib.PE;

namespace HoLLy.dnSpyExtension.Native
{
    public class ExportTable
    {
        public uint Timestamp;
        public ushort VersionMajor, VersionMinor;
        public string Name;
        public (RVA address, string name)[] Exports;

        private ExportTable(DataReaderFactory factory, IRvaFileOffsetConverter rvaConverter, ImageDataDirectory dataDirectory)
        {
            var dataDirectoryOffset = (uint) rvaConverter.ToFileOffset(dataDirectory.VirtualAddress);
            var dataDirectoryReader = factory.CreateReader(dataDirectoryOffset, dataDirectory.Size);

            var exportFlags = dataDirectoryReader.ReadUInt32();
            Timestamp = dataDirectoryReader.ReadUInt32();
            VersionMajor = dataDirectoryReader.ReadUInt16();
            VersionMinor = dataDirectoryReader.ReadUInt16();
            var nameRVA = (RVA)dataDirectoryReader.ReadUInt32();
            var ordinalBase = dataDirectoryReader.ReadUInt32();
            var addressTableEntries = dataDirectoryReader.ReadUInt32();
            var numberOfNamePointers = dataDirectoryReader.ReadUInt32();
            var exportAddressTableRVA = (RVA)dataDirectoryReader.ReadUInt32();
            var namePointerRVA = (RVA)dataDirectoryReader.ReadUInt32();
            var ordinalTableRVA = (RVA)dataDirectoryReader.ReadUInt32();

            var nameOffset = (uint)rvaConverter.ToFileOffset(nameRVA);
            var exportTableOffset = (uint)rvaConverter.ToFileOffset(exportAddressTableRVA);
            var namePointerOffset = (uint)rvaConverter.ToFileOffset(namePointerRVA);
            var ordinalTableOffset = (uint)rvaConverter.ToFileOffset(ordinalTableRVA);

            var reader = factory.CreateReader();

            var exportAddressTableReader = reader.Slice(exportTableOffset, 4 * addressTableEntries);
            var exportAddressTable = ReadRVAArray(exportAddressTableReader, addressTableEntries);

            var exportNamePointerTableReader = reader.Slice(namePointerOffset, 4 * numberOfNamePointers);
            var exportNamePointerTable = ReadRVAArray(exportNamePointerTableReader, numberOfNamePointers);

            // contains the index of the name: name = names[ordinal[i]]
            var exportOrdinalTableReader = reader.Slice(ordinalTableOffset, 2 * addressTableEntries);
            var exportOrdinalTable = ReadUShortArray(exportOrdinalTableReader, addressTableEntries);

            var exportNames = exportNamePointerTable
                .Select(rvaConverter.ToFileOffset)
                .Cast<uint>()
                .Select(o =>
                {
                    reader.Position = o;
                    return reader.TryReadZeroTerminatedString(Encoding.ASCII);
                })
                .ToArray();

            Exports = Enumerable
                .Range(0, exportAddressTable.Length)
                .Select(i => (
                    address: exportAddressTable[i], // TODO: forwarder RVAs
                    name: exportNames[exportOrdinalTable[i]]
                ))
                .ToArray();

            reader.Position = nameOffset;
            Name = reader.TryReadZeroTerminatedString(Encoding.ASCII);
        }

        private static RVA[] ReadRVAArray(DataReader reader, uint count)
        {
            var exportTable = new RVA[count];
            for (var i = 0; i < count; i++)
                exportTable[i] = (RVA)reader.ReadUInt32();

            return exportTable;
        }

        private static ushort[] ReadUShortArray(DataReader reader, uint count)
        {
            var exportTable = new ushort[count];
            for (var i = 0; i < count; i++)
                exportTable[i] = reader.ReadUInt16();

            return exportTable;
        }

        public static ExportTable Read(DataReaderFactory reader, IRvaFileOffsetConverter rvaConverter, ImageDataDirectory dataDirectory)
            => new(reader, rvaConverter, dataDirectory);
    }
}