using dnlib.DotNet;

namespace HoLLy.dnSpyExtension.Common.SourceMap
{
    internal interface ISourceMapStorage
    {
        string CacheFolder { get; }

        string? GetName(IMemberDef member);
        void SetName(IMemberDef member, string name);
        void SaveTo(IAssembly assembly, string location);
        void LoadFrom(IAssembly assembly, string location);
    }
}