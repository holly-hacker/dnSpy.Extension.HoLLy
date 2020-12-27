using System;
using System.Linq;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.TreeView;

namespace HoLLy.dnSpyExtension.Common.Commands
{
    public abstract class SingleTreeViewMenuItemBase : MenuItemBase<TreeNodeData>
    {
        protected override object CachedContextKey => new();

        protected override TreeNodeData? CreateContext(IMenuItemContext context)
        {
            // Make sure it's the file treeview
            if (context.CreatorObject.Guid != new Guid(MenuConstants.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID))
                return null;

            var selectedNodes = context.Find<TreeNodeData[]>();
            if (selectedNodes.Length != 1)
                return null;

            return selectedNodes.Single();
        }
    }
}