using System.ComponentModel.Composition;
using dnSpy.Contracts.Documents;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;
using Microsoft.Win32;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenuItem(Header = "Load SourceMap", Group = Constants.AppMenuGroupSourceMapSaveLoad, OwnerGuid = Constants.AppMenuGroupSourceMap, Order = 20)]
    internal class LoadSourceMap : MenuItemBase
    {
        private readonly ISourceMapStorage map;
        private readonly IDocumentTabService tabService;

        [ImportingConstructor]
        public LoadSourceMap(ISourceMapStorage map, IDocumentTabService tabService)
        {
            this.map = map;
            this.tabService = tabService;
        }

        public override void Execute(IMenuItemContext context)
        {
            var doc = GetDocument();
            var asm = doc?.AssemblyDef;
            if (asm is null) return;

            var ofd = new OpenFileDialog {
                Title = $"Save sourcemap for {asm.FullName}",
                FileName = $"{asm.Name}.xml",
                Filter = "SourceMap XML|*.xml|All Files|*"
            };

            if (ofd.ShowDialog() != true)
                return;

            map.LoadFrom(asm, ofd.FileName);
            tabService.RefreshModifiedDocument(doc!);
        }

        private IDsDocument? GetDocument() => tabService.DocumentTreeView.TreeView.SelectedItem?.GetDocumentNode()?.Document;

        public override bool IsEnabled(IMenuItemContext context) => GetDocument()?.AssemblyDef != null;
    }
}
