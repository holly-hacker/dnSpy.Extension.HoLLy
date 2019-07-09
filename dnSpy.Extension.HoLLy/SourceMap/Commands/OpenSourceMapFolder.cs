using System.ComponentModel.Composition;
using System.Diagnostics;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenuItem(Header = "Open SourceMap Cache Folder", Group = Constants.AppMenuGroupSourceMapSaveLoad, OwnerGuid = Constants.AppMenuGroupSourceMap, Order = 30)]
    internal class OpenSourceMapFolder : MenuItemBase
    {
        private readonly ISourceMapStorage sourcemap;

        [ImportingConstructor]
        public OpenSourceMapFolder(ISourceMapStorage sourcemap) => this.sourcemap = sourcemap;
        public override void Execute(IMenuItemContext context) => Process.Start(new ProcessStartInfo("explorer.exe", sourcemap.CacheFolder) { UseShellExecute = false });
    }
}
