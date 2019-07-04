using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using dnlib.DotNet;
using dnSpy.Contracts.App;

namespace HoLLy.dnSpyExtension.SourceMap
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

        public string GetName(IMemberDef member)
        {
            var asm = member.Module.Assembly;

            if (!loadedMaps.ContainsKey(asm) && !Load(asm))
                return null;

            if (loadedMaps[asm] == null)
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

            if (!loadedMaps.ContainsKey(asm) || loadedMaps[asm] == null)
                loadedMaps[asm] = new Dictionary<(MapType, string), string>();

            var map = loadedMaps[asm];
            var key = (GetMapType(member), member.FullName);

            map[key] = name;
        }

        public void Save()
        {
            // TODO: handle inability to save
            foreach (var asmMap in loadedMaps.Where(x => x.Value != null)) {
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

        private bool Load(AssemblyDef asm)
        {
            // TODO: gracefully fail
            string path = GetStorageLocation(asm);

            if (!File.Exists(path)) {
                loadedMaps[asm] = null;
                return false;
            }

            var dic = new Dictionary<(MapType, string), string>();

            using var reader = XmlReader.Create(path);
            while (reader.Read()) {
                if (reader.IsStartElement()) {
                    if (Enum.TryParse(reader.Name, true, out MapType type)) {
                        string orig = reader["original"];
                        string mapped = reader["mapped"];

                        dic[(type, orig)] = mapped;
                    }
                }
            }

            loadedMaps[asm] = dic;
            return true;
        }

        private string GetStorageLocation(AssemblyDef asm)
        {
            string directory = Path.Combine(AppDirectories.DataDirectory, "SourceMaps");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return Path.Combine(directory, $"{asm.FullName}.xml");
        }

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
