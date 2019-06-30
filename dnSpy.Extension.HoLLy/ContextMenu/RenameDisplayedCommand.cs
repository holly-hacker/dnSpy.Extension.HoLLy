using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpy.Extension.Decompilers.Decorators;

namespace HoLLy.dnSpy.Extension.ContextMenu
{
    [ExportMenuItem(Header = "Change displayed name", Group = Constants.ContextMenuGroupEdit)]
    public class RenameDisplayedCommand : MenuItemBase
    {
        public override void Execute(IMenuItemContext context)
        {
            var textReference = context.Find<TextReference>();
            var docViewer = context.Find<IDocumentViewer>();
            var m = (IMemberDef)textReference.Reference;

            string newName = MsgBox.Instance.Ask<string>($"New name for {m.Name}:");
            if (!string.IsNullOrEmpty(newName))
                TempRenameCache.SetName(m, newName);

            // BUG: this is not enough to re-decompile, there must be some cache
            var documentTabService = docViewer.DocumentTab.DocumentTabService;
            RefreshTabs(documentTabService);
        }

        private static void RefreshTabs(IDocumentTabService documentTabService)
        {
            IEnumerable<(IDocumentTab tab, IDecompiler decompiler)> decompilerTabs = documentTabService.VisibleFirstTabs.Select(tab => new {
                    tab,
                    decompiler = (tab.Content as IDecompilerTabContent)?.Decompiler
                })
                .Where(t => !(t.decompiler is null))
                .Select(t => (t.tab, t.decompiler));

            documentTabService.Refresh(decompilerTabs.Where(t => t.decompiler is DecompilerDecorator).Select(a => a.tab).ToArray());
        }

        public override bool IsVisible(IMenuItemContext context)
        {
            var tf = context.Find<TextReference>();
            return tf?.Reference is IMemberDef;
        }
    }
}
