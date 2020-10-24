using System.Text;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;

namespace HoLLy.dnSpyExtension.Common.Commands.Debug
{
    [ExportMenuItem(Header = "DBG: View context", Group = Constants.ContextMenuGroupDebug)]
    public class ShowContext : MenuItemBase
    {
        public override bool IsVisible(IMenuItemContext context) => Utils.IsDebugBuild;

        public override void Execute(IMenuItemContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CreatorObject:");
            sb.AppendLine($" - {context.CreatorObject.Guid} {context.CreatorObject.Object}");
            sb.AppendLine();

            sb.AppendLine("GuidObjects:");
            foreach (GuidObject obj in context.GuidObjects)
                sb.AppendLine($" - {obj.Guid} {obj.Object}");
            sb.AppendLine();

            // if we have access to an IDocumentViewer, try to find the clicked item
            TextReference? textRef = context.Find<TextReference?>();
            if (textRef != null)
                sb.AppendLine($"TextReference.Reference type: {textRef.Reference?.GetType()}");

            MsgBox.Instance.Show(sb.ToString());
        }

        public override string GetHeader(IMenuItemContext context)
        {
            return $"DBG: View context of {context.CreatorObject.Object?.GetType().Name}";
        }
    }
}
