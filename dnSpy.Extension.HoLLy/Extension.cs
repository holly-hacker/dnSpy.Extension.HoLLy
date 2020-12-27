using System.Collections.Generic;
using dnSpy.Contracts.Extension;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension
{
    [ExportExtension]
    public class Extension : IExtension
    {
        static Extension()
        {
            Utils.InstallLocalAssemblyResolver();
        }

        public IEnumerable<string> MergedResourceDictionaries { get { yield break; } }

        public ExtensionInfo ExtensionInfo => new() {
            ShortDescription = "HoLLy's extension" + (Utils.IsDebugBuild ? " [DEBUG]" : string.Empty),
        };

        public void OnEvent(ExtensionEvent e, object? obj) { }
    }
}
