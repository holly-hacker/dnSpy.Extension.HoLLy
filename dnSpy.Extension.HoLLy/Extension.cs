using System.Collections.Generic;
using dnSpy.Contracts.Extension;

namespace HoLLy.dnSpy.Extension
{
    [ExportExtension]
    public class Extension : IExtension
    {
        public IEnumerable<string> MergedResourceDictionaries { get { yield break; } }

        public ExtensionInfo ExtensionInfo => new ExtensionInfo {
                ShortDescription = "HoLLy's extension"
            };

        public void OnEvent(ExtensionEvent e, object obj) { }
    }
}
