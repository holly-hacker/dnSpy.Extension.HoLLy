using System.Text;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.TreeView;

namespace HoLLy.dnSpyExtension.Common.Commands.Debug
{
    [ExportMenuItem(Header = "DBG: View tree node type", Group = Constants.ContextMenuGroupDebug)]
    public class ShowTreeNodeType : SingleTreeViewMenuItemBase
    {
        public override bool IsVisible(TreeNodeData context) => Utils.IsDebugBuild;

        public override string GetHeader(TreeNodeData context) => $"DBG: {context.GetType().Name} info";

        public override void Execute(TreeNodeData context)
        {
            var sb = new StringBuilder();

            sb.AppendLine("TreeNodeData guid: " + context.Guid);
            sb.AppendLine("TreeNodeData type: " + context.GetType());

            MsgBox.Instance.Show(sb.ToString());
        }

    }
}