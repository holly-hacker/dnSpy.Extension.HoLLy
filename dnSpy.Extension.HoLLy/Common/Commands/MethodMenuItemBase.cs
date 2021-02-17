using System;
using System.Linq;
using dnlib.DotNet;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.TreeView;

namespace HoLLy.dnSpyExtension.Common.Commands
{
    public abstract class MethodMenuItemBase : MenuItemBase
    {
        protected virtual bool IsVisible(MethodDef md) => true;
        protected virtual bool IsEnabled(MethodDef md) => true;
        protected abstract void Execute(MethodDef md);

        public sealed override bool IsVisible(IMenuItemContext context)
        {
            var method = GetMethod(context);
            return method is not null && IsVisible(method);
        }

        public sealed override bool IsEnabled(IMenuItemContext context)
        {
            var method = GetMethod(context);
            return method is not null && IsEnabled(method);
        }

        public sealed override void Execute(IMenuItemContext context)
        {
            var method = GetMethod(context);
            if (method is null)
                throw new Exception(
                    $"Tried to execute a {nameof(MethodMenuItemBase)} but got null {nameof(MethodDef)}");
            Execute(method);
        }

        private static MethodDef? GetMethod(IMenuItemContext context)
        {
            if (IsTreeView(context))
            {
                var selectedNodes = context.Find<TreeNodeData[]>();
                if (selectedNodes.Length != 1)
                    return null;

                var node = selectedNodes.Single();
                return node.TreeNode.Data is MethodNode mn ? mn.MethodDef : null;
            }
            else
            {
                return context.Find<TextReference>()?.Reference is MethodDef md ? md : null;
            }
        }

        private static bool IsTreeView(IMenuItemContext context)
        {
            return context.CreatorObject.Guid == new Guid(MenuConstants.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID);
        }
    }
}