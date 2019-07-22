using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.CodeInjection.Commands.Debug
{
    [ExportMenuItem(Header = "DBG: Inject DLL", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerDebug)]
    internal class InjectDLLDebug : MenuItemBase
    {
        private readonly IManagedInjector injector;

        [ImportingConstructor]
        public InjectDLLDebug(IManagedInjector injector) => this.injector = injector;

        public override void Execute(IMenuItemContext context)
        {
            var pid = MsgBox.Instance.Ask<int?>("Process ID", verifier: (valStr) => {
                var val = (int?)TypeDescriptor.GetConverter(typeof(int?)).ConvertFromInvariantString(valStr);
                if (val is null) return "Not an int?";
                if (Process.GetProcesses().All(p => p.Id != val.Value)) return $"Couldn't find process with id {val.Value} (0x{val.Value:X})";
                return null;
            });
            if (pid is null) return;

            var rt = MsgBox.Instance.Ask<RuntimeType?>("Runtime type?");
            if (rt is null) return;

            if (!InjectDLL.AskForEntryPoint(out InjectionArguments args))
                return;

            injector.Log = s => MsgBox.Instance.Show(s);
            injector.Inject(pid.Value, args, IntPtr.Size == 4, rt.Value);
        }

        public override bool IsVisible(IMenuItemContext context) => Utils.IsDebugBuild;
    }
}
