using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Text;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.CodeInjection.Commands
{
    [ExportMenuItem(OwnerGuid = Constants.AppMenuGuidDebuggerInjectRecent, Group = Constants.AppMenuGroupDebuggerInjectRecent, Order = 0)]
    internal class InjectRecentItems : MenuItemBase, IMenuItemProvider
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private DbgProcess CurrentProcess => DbgManager.CurrentProcess.Current
                                             ?? DbgManager.Processes.FirstOrDefault();

        private readonly Lazy<DbgManager> dbgManagerLazy;
        private readonly IManagedInjector injector;
        private readonly Settings settings;

        [ImportingConstructor]
        public InjectRecentItems(Lazy<DbgManager> dbgManagerLazy, IManagedInjector injector, Settings settings)
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
            private readonly InjectRecentItems parent;
            private readonly InjectionArguments injectionArguments;

            public InjectRecentMenuItem(InjectRecentItems parent, InjectionArguments injectionArguments)
            {
                this.parent = parent;
                this.injectionArguments = injectionArguments;
            }

            /// <remarks>
            /// TODO: Use UIUtilities.EscapeMenuItemHeader, see 0xd4d/dnSpy#1201
            /// </remarks>
            public override string GetHeader(IMenuItemContext context) => NameUtilities
                .CleanName($"{Path.GetFileName(injectionArguments.Path)} {injectionArguments.TypeFull}.{injectionArguments.Method}({Quote(injectionArguments.Argument) ?? "null"})")
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