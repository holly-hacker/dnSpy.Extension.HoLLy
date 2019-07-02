using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnlib.DotNet;
using System.IO;
using System.Xml;
using dnSpy.Contracts.Documents;

namespace HoLLy.dnSpy.Extension.SourceMap
{
    internal interface ISourceMapStorage
    {
        string GetName(IMemberDef member);
        void SetName(IMemberDef member, string name);
        void Save();
    }

    [Export(typeof(ISourceMapStorage))]
    internal class SourceMapStorage : ISourceMapStorage
    {
        private readonly Dictionary<AssemblyDef, Dictionary<(MapType, string), string>> loadedMaps = new Dictionary<AssemblyDef, Dictionary<(MapType, string), string>>();
        private readonly Settings settings;
        private readonly IDsDocumentService docService;

        [ImportingConstructor]
        public SourceMapStorage(Settings s, IDsDocumentService docService)
        {
            settings = s;
            this.docService = docService;
        }
        public string GetName(IMemberDef member)
        {
            var asm = member.Module.Assembly;

            if (!loadedMaps.ContainsKey(asm))
                return null;

            var map = loadedMaps[asm];
            var key = (GetMapType(member), member.FullName);

            if (!map.ContainsKey(key))
                return null;

            return map[key];

        }

        public void SetName(IMemberDef member, string name)
        {
            var asm = member.Module.Assembly;

            if (!loadedMaps.ContainsKey(asm))
                loadedMaps[asm] = new Dictionary<(MapType, string), string>();

            var map = loadedMaps[asm];
            var key = (GetMapType(member), member.FullName);

            map[key] = name;
        }

        public void Save()
        {
            // TODO: handle inability to save
            if (settings.SourceMapStorageLocation == StorageLocation.None)
                return;

            foreach (var asmMap in loadedMaps) {
                (AssemblyDef asm, Dictionary<(MapType, string), string> map) = (asmMap.Key, asmMap.Value);
                string path = GetStorageLocation(asm);

                // would like to not depend on any nuget packages, so not using Json.NET or anything .NET Core specific
                using var writer = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
                writer.WriteStartDocument();
                writer.WriteStartElement("SourceMap");

                foreach (var pair in map) {
                    (MapType type, string original, string mapped) = (pair.Key.Item1, pair.Key.Item2, pair.Value);

                    writer.WriteStartElement(type.ToString());
                    writer.WriteAttributeString("original", original);
                    writer.WriteAttributeString("mapped", mapped);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private string GetStorageLocation(AssemblyDef asm)
        {
            // TODO: handle null
            string directory = settings.SourceMapStorageLocation switch {
                StorageLocation.AssemblyLocation => Path.GetDirectoryName(GetAssemblyLocation(asm)),
                _ => throw new ArgumentOutOfRangeException(nameof(settings.SourceMapStorageLocation), "Unknown storage type: " + settings.SourceMapStorageLocation),
            };

            return Path.Combine(directory, $"{asm.FullName}.sourcemap.xml");
        }

        private string GetAssemblyLocation(AssemblyDef asm) => docService.FindAssembly(asm).Filename;

        private static MapType GetMapType(IMemberDef member) =>
            member switch {
                MethodDef _ => MapType.MethodDef,
                TypeDef _ => MapType.TypeDef,
                FieldDef _ => MapType.FieldDef,
                PropertyDef _ => MapType.PropertyDef,
                _ => MapType.Other,
            };

        private enum MapType
        {
            Other,
            MethodDef,
            TypeDef,
            FieldDef,
            PropertyDef,
        }
    }
}
