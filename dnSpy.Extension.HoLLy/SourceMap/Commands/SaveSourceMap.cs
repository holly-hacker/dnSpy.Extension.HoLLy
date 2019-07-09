using System.ComponentModel.Composition;
using System.Windows.Forms;
using dnSpy.Contracts.Documents;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenuItem(Header = "Save SourceMap", Group = Constants.AppMenuGroupSourceMapSaveLoad, OwnerGuid = Constants.AppMenuGroupSourceMap, Order = 10)]
    internal class SaveSourceMap : MenuItemBase
    {
        private readonly ISourceMapStorage map;
        private readonly IDocumentTabService tabService;

        [ImportingConstructor]
        public SaveSourceMap(ISourceMapStorage map, IDocumentTabService tabService)
        {
            this.map = map;
            this.tabService = tabService;
        }

        public override void Execute(IMenuItemContext context)
        {
            var asm = GetDocument()?.AssemblyDef;
            if (asm is null) return;

            var sfd  = new SaveFileDialog {
                Title = $"Save sourcemap for {asm.FullName}",
                FileName = $"{asm.Name}.xml",
                Filter = "SourceMap XML|*.xml|All Files|*"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            map.SaveTo(asm, sfd.FileName);
        }

        private IDsDocument? GetDocument() => tabService.DocumentTreeView.TreeView.SelectedItem?.GetDocumentNode()?.Document;

        public override bool IsEnabled(IMenuItemContext context) => GetDocument() != null;
    }
}
