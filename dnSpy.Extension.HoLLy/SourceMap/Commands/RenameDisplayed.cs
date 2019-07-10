using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenuItem(Header = "Change displayed name", Group = Constants.ContextMenuGroupEdit)]
    internal class RenameDisplayed : MenuItemBase
    {
        private readonly ISourceMapStorage sourceMapStorage;

        [ImportingConstructor]
        public RenameDisplayed(ISourceMapStorage sourceMapStorage)
        {
            this.sourceMapStorage = sourceMapStorage;
        }

        public override void Execute(IMenuItemContext context)
        {
            var textReference = context.Find<TextReference>();
            var docViewer = context.Find<IDocumentViewer>();
            var m = (IMemberDef)textReference.Reference;

            static string getName(IMemberDef md) => (md is MethodDef methodDef && methodDef.IsConstructor) ? getName(methodDef.DeclaringType) : (string)md.Name;
            string newName = MsgBox.Instance.Ask<string>(string.Empty, title: $"New name for {getName(m)}");

            if (string.IsNullOrEmpty(newName))
                return;

            sourceMapStorage.SetName(m, newName);

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
