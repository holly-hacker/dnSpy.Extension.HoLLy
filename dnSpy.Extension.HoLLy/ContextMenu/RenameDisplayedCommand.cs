using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;

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

            var documentTabService = docViewer.DocumentTab.DocumentTabService;
            documentTabService.RefreshModifiedDocument(documentTabService.DocumentTreeView.FindNode(m.Module).Document);
        }

        public override bool IsVisible(IMenuItemContext context)
        {
            var tf = context.Find<TextReference>();
            return tf?.Reference is IMemberDef;
        }
    }
}
