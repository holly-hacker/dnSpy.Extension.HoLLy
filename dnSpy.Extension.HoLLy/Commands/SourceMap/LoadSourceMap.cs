using System.ComponentModel.Composition;
using System.Windows.Forms;
using dnSpy.Contracts.Documents;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.SourceMap;

namespace HoLLy.dnSpyExtension.Commands.SourceMap
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
            if (doc is null) return;

            var asm = doc.AssemblyDef;
            var ofd = new OpenFileDialog {
                Title = $"Save sourcemap for {asm.FullName}",
                FileName = $"{asm.Name}.xml",
                Filter = "SourceMap XML|*.xml|All Files|*"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            map.LoadFrom(asm, ofd.FileName);
            tabService.RefreshModifiedDocument(doc);
        }

        private IDsDocument? GetDocument() => tabService.DocumentTreeView.TreeView.SelectedItem?.GetDocumentNode()?.Document;

        public override bool IsEnabled(IMenuItemContext context) => GetDocument() != null;
    }
}
