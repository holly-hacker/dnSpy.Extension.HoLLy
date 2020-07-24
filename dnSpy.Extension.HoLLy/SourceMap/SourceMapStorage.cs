using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;

namespace HoLLy.dnSpyExtension.SourceMap
{
    [Export(typeof(ISourceMapStorage))]
    internal class SourceMapStorage : ISourceMapStorage
    {
        public string CacheFolder => Path.Combine(AppDirectories.DataDirectory, "SourceMaps");

        private readonly Dictionary<IAssembly, Dictionary<(MapType, string), string>?> loadedMaps = new Dictionary<IAssembly, Dictionary<(MapType, string), string>?>();
        private readonly Settings settings;

        [ImportingConstructor]
        public SourceMapStorage(Settings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Tries to resolve the mapped name of a <see cref="IMemberDef"/>.
        /// </summary>
        /// <returns>The mapped name, or <c>null</c> if it was not found.</returns>
        public string? GetName(IMemberDef member)
        {
            member = SourceMapUtils.GetDefToMap(member);

            var asm = member.Module.Assembly;

            // null if we tried to load from cache before, to prevent excessive fs access
            if (loadedMaps.ContainsKey(asm) && loadedMaps[asm] == null)
                return null;

            if (!loadedMaps.ContainsKey(asm) && !TryLoadFromCache(asm))
                return null;

            var map = loadedMaps[asm]!;
            var key = (GetMapType(member), member.FullName);

            if (map.ContainsKey(key))
                return map[key];

            // if there was no mapped name found, see if we can get a sensible name in another way
            if (member is MethodDef md) {
                if (settings.AutoMapDLLImports && md.HasImplMap)
                    return md.ImplMap.Name;

                if (settings.AutoMapOverrides && md.HasOverrides) {
                    var firstOverride = md.Overrides.First().MethodDeclaration;

                    // if the implementation is in another assembly, don't assume we mapped it
                    // TODO: try to resolve this to a IMemberDef so we can recursively find name?
                    if (!(firstOverride is IMemberDef))
                        return firstOverride.Name;
                }

                // TODO: interfaces can be implemented without overrides
            }

            return null;
        }

        /// <summary>
        /// Stores a new mapped name for this <paramref name="member"/> in the local cache, and saves it.
        /// </summary>
        public void SetName(IMemberDef member, string name)
        {
            member = SourceMapUtils.GetDefToMap(member);

            var asm = member.Module.Assembly;

            if (!loadedMaps.ContainsKey(asm) || loadedMaps[asm] == null)
                loadedMaps[asm] = new Dictionary<(MapType, string), string>();

            var map = loadedMaps[asm]!;
            var key = (GetMapType(member), member.FullName);
            map[key] = name;

            SaveTo(asm, GetCacheLocation(asm));
        }

        /// <summary>
        /// Saves the local cache for <paramref name="assembly"/> to <paramref name="location"/>.
        /// </summary>
        public void SaveTo(IAssembly assembly, string location)
        {
            if (!loadedMaps.ContainsKey(assembly))
                if (!TryLoadFromCache(assembly))
                    throw new Exception("No sourcemap found for this assembly");

            var map = loadedMaps[assembly] ?? new Dictionary<(MapType, string), string>();

            // TODO: handle inability to save
            // would like to not depend on any nuget packages, so not using Json.NET or anything .NET Core specific
            using var writer = XmlWriter.Create(location, new XmlWriterSettings { Indent = true });
            writer.WriteStartDocument();
            writer.WriteStartElement("SourceMap");

            foreach (var pair in map) {
                (MapType type, string original, string mapped) = (pair.Key.Item1, pair.Key.Item2, pair.Value);

                writer.WriteStartElement(type.ToString());

                // store as base64 if there is a character under 0x20
                // TODO: not perfect, see https://referencesource.microsoft.com/#system.xml/System/Xml/Core/XmlUtf8RawTextWriter.cs,675 for real handling
                if (original.Any(ch => ch < ' '))
                {
                    writer.WriteAttributeString("encoding", "base64");
                    writer.WriteAttributeString("original", Convert.ToBase64String(Encoding.UTF8.GetBytes(original)));
                }
                else
                {
                    writer.WriteAttributeString("original", original);
                }

                writer.WriteAttributeString("mapped", mapped);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        /// <summary>
        /// Loads the cache for <paramref name="assembly"/> from <paramref name="location"/>, then immediately saves it
        /// to the default cache location if it wasn't loaded from there.
        /// </summary>
        public void LoadFrom(IAssembly assembly, string location)
        {
            LoadFromInternal(assembly, location);

            // save to default location if it was not loaded from there already
            var defaultLocation = GetCacheLocation(assembly);
            if (!string.Equals(Path.GetFullPath(location), Path.GetFullPath(defaultLocation), StringComparison.InvariantCultureIgnoreCase))
                SaveTo(assembly, defaultLocation);
        }

        private bool TryLoadFromCache(IAssembly assembly)
        {
            string location = GetCacheLocation(assembly);

            if (!File.Exists(location)) {
                loadedMaps[assembly] = null;
                return false;
            }

            LoadFromInternal(assembly, location);
            return true;
        }

        private void LoadFromInternal(IAssembly assembly, string location)
        {
            // TODO: gracefully fail
            var dic = new Dictionary<(MapType, string), string>();

            using (var reader = XmlReader.Create(location)) {
                while (reader.Read()) {
                    if (reader.IsStartElement()) {
                        if (Enum.TryParse(reader.Name, true, out MapType type))
                        {
                            string orig = reader.GetAttribute("encoding") == "base64"
                                ? Encoding.UTF8.GetString(Convert.FromBase64String(reader["original"]))
                                : reader["original"];
                            string mapped = reader["mapped"];

                            dic[(type, orig)] = mapped;
                        }
                    }
                }
            }

            loadedMaps[assembly] = dic;
        }

        private string GetCacheLocation(IAssembly asm)
        {
            string directory = CacheFolder;

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
