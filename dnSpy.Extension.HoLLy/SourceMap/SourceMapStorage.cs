using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml;
using dnlib.DotNet;
using dnSpy.Contracts.App;

namespace HoLLy.dnSpyExtension.SourceMap
{
    internal interface ISourceMapStorage
    {
        string StorageFolder { get; }

        string GetName(IMemberDef member);
        void SetName(IMemberDef member, string name);
        void SaveTo(IAssembly assembly, string location);
        void LoadFrom(IAssembly assembly, string location);
    }

    [Export(typeof(ISourceMapStorage))]
    internal class SourceMapStorage : ISourceMapStorage
    {
        public string StorageFolder => Path.Combine(AppDirectories.DataDirectory, "SourceMaps");

        private readonly Dictionary<IAssembly, Dictionary<(MapType, string), string>> loadedMaps = new Dictionary<IAssembly, Dictionary<(MapType, string), string>>();

        public string GetName(IMemberDef member)
        {
            var asm = member.Module.Assembly;

            if (!loadedMaps.ContainsKey(asm) && !TryLoad(asm, GetStorageLocation(asm)))
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

            SaveTo(asm, GetStorageLocation(asm));
        }

        public void SaveTo(IAssembly assembly, string location)
        {
            // TODO: handle inability to save
            // would like to not depend on any nuget packages, so not using Json.NET or anything .NET Core specific
            using var writer = XmlWriter.Create(location, new XmlWriterSettings { Indent = true });
            writer.WriteStartDocument();
            writer.WriteStartElement("SourceMap");

            foreach (var pair in loadedMaps[assembly]) {
                (MapType type, string original, string mapped) = (pair.Key.Item1, pair.Key.Item2, pair.Value);

                writer.WriteStartElement(type.ToString());
                writer.WriteAttributeString("original", original);
                writer.WriteAttributeString("mapped", mapped);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public void LoadFrom(IAssembly assembly, string location)
        {
            // TODO: gracefully fail
            var dic = new Dictionary<(MapType, string), string>();

            using var reader = XmlReader.Create(location);
            while (reader.Read()) {
                if (reader.IsStartElement()) {
                    if (Enum.TryParse(reader.Name, true, out MapType type)) {
                        string orig = reader["original"];
                        string mapped = reader["mapped"];

                        dic[(type, orig)] = mapped;
                    }
                }
            }

            loadedMaps[assembly] = dic;
        }

        private bool TryLoad(IAssembly assembly, string location)
        {
            if (!File.Exists(location)) {
                loadedMaps[assembly] = null;
                return false;
            }

            LoadFrom(assembly, location);
            return true;
        }

        private string GetStorageLocation(IAssembly asm)
        {
            string directory = StorageFolder;

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
