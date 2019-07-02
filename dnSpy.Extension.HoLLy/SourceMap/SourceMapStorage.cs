using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnlib.DotNet;
using System.IO;
using System.Xml;
using dnSpy.Contracts.App;

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
