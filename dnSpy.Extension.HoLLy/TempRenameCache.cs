using System.Collections.Generic;
using dnlib.DotNet;

namespace HoLLy.dnSpy.Extension
{
    public static class TempRenameCache
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Cache = new Dictionary<string, Dictionary<string, string>>();

        public static string GetNameOrDefault(IMemberDef member)
        {
            var asmName = member.GetType().AssemblyQualifiedName ?? "";
            if (!Cache.ContainsKey(asmName))
                return member.Name;

            var innerDic = Cache[asmName];
            if (!innerDic.ContainsKey(member.FullName))
                return member.Name;

            return innerDic[member.FullName];

        }

        public static void SetName(IMemberDef member, string name)
        {
            string asmName = member.GetType().AssemblyQualifiedName ?? "";

            if (!Cache.ContainsKey(asmName))
                Cache[asmName] = new Dictionary<string, string>();

            Cache[asmName][member.FullName] = name;
        }
    }
}
