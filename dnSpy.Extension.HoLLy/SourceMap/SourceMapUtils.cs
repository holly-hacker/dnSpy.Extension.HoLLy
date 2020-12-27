using dnlib.DotNet;

namespace HoLLy.dnSpyExtension.SourceMap
{
    public static class SourceMapUtils
    {
        public static IMemberDef GetDefToMap(IMemberDef def)
        {
            if (def is MethodDef {IsConstructor: true})
                return def.DeclaringType;

            return def;
        }
    }
}