using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnlib.DotNet;

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

        [ImportingConstructor]
        public SourceMapStorage(Settings s)
        {
            settings = s;
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
            switch (settings.SourceMapStorageLocation) {
                case StorageLocation.None: break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static MapType GetMapType(IMemberDef member) =>
            member switch {
                MethodDef _ => MapType.MethodDef,
                TypeDef _ => MapType.TypeDef,
                FieldDef _ => MapType.FieldDef,
                PropertyDef _ => MapType.PropertyDef,
                _ => MapType.None,
            };

        private enum MapType
        {
            None,
            MethodDef,
            TypeDef,
            FieldDef,
            PropertyDef,
        }
    }
}
