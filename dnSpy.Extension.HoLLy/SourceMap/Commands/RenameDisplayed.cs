using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;
using HoLLy.dnSpyExtension.SourceMap.Decompilers;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenuItem(Header = "Change displayed name", Group = Constants.ContextMenuGroupEdit)]
    internal class RenameDisplayed : MenuItemBase
    {
        private readonly ISourceMapStorage sourceMapStorage;
        private readonly IDecompilerService decompilerService;

        [ImportingConstructor]
        public RenameDisplayed(ISourceMapStorage sourceMapStorage, IDecompilerService decompilerService)
        {
            this.sourceMapStorage = sourceMapStorage;
            this.decompilerService = decompilerService;
        }

        public override void Execute(IMenuItemContext context)
        {
            var textReference = context.Find<TextReference?>();
            var docViewer = context.Find<IDocumentViewer?>();

            if (textReference?.Reference is not IMemberDef m)
                return;

            string newName = MsgBox.Instance.Ask<string>(string.Empty, title: $"New name for {SourceMapUtils.GetDefToMap(m)}");

            if (string.IsNullOrEmpty(newName))
                return;

            sourceMapStorage.SetName(m, newName);

            var documentTabService = docViewer?.DocumentTab?.DocumentTabService;
            documentTabService?.RefreshModifiedDocument(documentTabService.DocumentTreeView.FindNode(m.Module)!.Document);
        }

        public override bool IsVisible(IMenuItemContext context)
        {
            if (!(decompilerService.Decompiler is SourceMapDecompilerDecorator))
                return false;

            return context.Find<TextReference?>()?.Reference is IMemberDef;
        }
    }
}
