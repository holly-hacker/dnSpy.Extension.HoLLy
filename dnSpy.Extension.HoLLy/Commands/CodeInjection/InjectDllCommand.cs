using System;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpy.Extension.CodeInjection;

namespace HoLLy.dnSpy.Extension.Commands.CodeInjection
{
    [ExportMenuItem(Header = "Inject DLL", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerInject)]
    internal class InjectDllCommand : MenuItemBase
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private readonly Lazy<DbgManager> dbgManagerLazy;
        private readonly ManagedInjector injector;

        [ImportingConstructor]
        public InjectDllCommand(Lazy<DbgManager> dbgManagerLazy, ManagedInjector injector)
        {
            this.dbgManagerLazy = dbgManagerLazy;
            this.injector = injector;
        }

        public override void Execute(IMenuItemContext context)
        {
            // TODO: pass DLL
            injector.Inject();
        }

        public override bool IsVisible(IMenuItemContext context) => DbgManager.IsDebugging;
        public override bool IsEnabled(IMenuItemContext context) => true;    // TODO: check if supported
    }
}
