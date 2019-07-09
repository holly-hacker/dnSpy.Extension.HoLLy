using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Text;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.Settings;

namespace HoLLy.dnSpyExtension.CodeInjection.Commands
{
    [ExportMenuItem(Header = "Re-inject recent", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerInject, Order = 20, Guid = Constants.AppMenuGuidDebuggerInjectRecent)]
    internal class InjectRecentMenu : MenuItemBase
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private DbgProcess CurrentProcess => DbgManager.CurrentProcess.Current
                                             ?? DbgManager.Processes.FirstOrDefault();

        private readonly Settings settings;
        private readonly Lazy<DbgManager> dbgManagerLazy;

        [ImportingConstructor]
        public InjectRecentMenu(Settings settings, Lazy<DbgManager> dbgManagerLazy)
        {
            this.settings = settings;
            this.dbgManagerLazy = dbgManagerLazy;
        }

        public override void Execute(IMenuItemContext context) { }

        public override bool IsEnabled(IMenuItemContext context) => DbgManager.IsDebugging;
        public override bool IsVisible(IMenuItemContext context) => settings.RecentInjections.Any() && ManagedInjector.IsProcessSupported(CurrentProcess, out _);
    }

    [ExportMenuItem(OwnerGuid = Constants.AppMenuGuidDebuggerInjectRecent, Group = Constants.AppMenuGroupDebuggerInjectRecent, Order = 0)]
    internal class InjectRecentMenuList : MenuItemBase, IMenuItemProvider
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private DbgProcess CurrentProcess => DbgManager.CurrentProcess.Current
                                             ?? DbgManager.Processes.FirstOrDefault();

        private readonly Lazy<DbgManager> dbgManagerLazy;
        private readonly ManagedInjector injector;
        private readonly Settings settings;

        [ImportingConstructor]
        public InjectRecentMenuList(Lazy<DbgManager> dbgManagerLazy, ManagedInjector injector, Settings settings)
        {
            this.dbgManagerLazy = dbgManagerLazy;
            this.injector = injector;
            this.settings = settings;
        }

        public override void Execute(IMenuItemContext context) { }

        public IEnumerable<CreatedMenuItem> Create(IMenuItemContext context)
        {
            foreach (var recentInjection in settings.RecentInjections)
                yield return new CreatedMenuItem(new ExportMenuItemAttribute { OwnerGuid = Constants.AppMenuGuidDebuggerInjectRecent }, new InjectRecentMenuItem(this, recentInjection));
        }

        private class InjectRecentMenuItem : MenuItemBase
        {
            private readonly InjectRecentMenuList parent;
            private readonly InjectionArguments injectionArguments;

            public InjectRecentMenuItem(InjectRecentMenuList parent, InjectionArguments injectionArguments)
            {
                this.parent = parent;
                this.injectionArguments = injectionArguments;
            }

            /// <remarks>
            /// TODO: Use UIUtilities.EscapeMenuItemHeader, see 0xd4d/dnSpy#1201
            /// </remarks>
            public override string GetHeader(IMenuItemContext context) => NameUtilities
                    .CleanName($"{Path.GetFileName(injectionArguments.Path)} {injectionArguments.Type}.{injectionArguments.Method}({Quote(injectionArguments.Argument) ?? "null"})")
                    .Replace("_", "__");
            public override bool IsEnabled(IMenuItemContext context) => File.Exists(injectionArguments.Path);

            public override void Execute(IMenuItemContext context)
            {
                parent.settings.AddRecentInjection(injectionArguments);
                DbgProcess proc = parent.CurrentProcess;
                parent.injector.Inject(proc.Id, injectionArguments, proc.Bitness == 32, proc.Runtimes.First().GetRuntimeType());
            }

            private static string? Quote(string? s) => s is null ? null : $"\"{s}\"";
        }
    }
}
