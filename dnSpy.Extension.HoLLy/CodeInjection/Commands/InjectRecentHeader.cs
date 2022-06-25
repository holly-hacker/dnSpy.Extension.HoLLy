using System;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.CodeInjection.Commands
{
    [ExportMenuItem(Header = "Re-inject recent", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerInject, Order = 20, Guid = Constants.AppMenuGuidDebuggerInjectRecent)]
    internal class InjectRecentHeader : MenuItemBase
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private DbgProcess? CurrentProcess => DbgManager.CurrentProcess.Current
                                              ?? DbgManager.Processes.FirstOrDefault();

        private readonly Settings settings;
        private readonly Lazy<DbgManager> dbgManagerLazy;
        private readonly IManagedInjector injector;

        [ImportingConstructor]
        public InjectRecentHeader(Settings settings, Lazy<DbgManager> dbgManagerLazy, IManagedInjector injector)
        {
            this.settings = settings;
            this.dbgManagerLazy = dbgManagerLazy;
            this.injector = injector;
        }

        public override void Execute(IMenuItemContext context) { }
        public override bool IsEnabled(IMenuItemContext context) => DbgManager.IsDebugging;
        public override bool IsVisible(IMenuItemContext context) => settings.RecentInjections.Any() && injector.IsProcessSupported(CurrentProcess, out _);
    }
}
