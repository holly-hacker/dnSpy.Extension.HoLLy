
using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.CodeInjection;

namespace HoLLy.dnSpyExtension.Commands.CodeInjection.Debug
{
    [ExportMenuItem(Header = "DBG: Inject DLL", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerDebug)]
    internal class InjectDllDebugCommand : MenuItemBase
    {
        private readonly ManagedInjector injector;

        [ImportingConstructor]
        public InjectDllDebugCommand(ManagedInjector injector) => this.injector = injector;

        public override void Execute(IMenuItemContext context)
        {
            var pid = MsgBox.Instance.Ask<int?>("Process ID");
            if (pid is null) return;

            var x86 = MsgBox.Instance.Ask<bool?>("Is x86?");
            if (x86 is null) return;

            if (!InjectDllCommand.AskForEntryPoint(out MethodDef method, out string parameter))
                return;

            injector.Inject(pid.Value, method, parameter, x86.Value, RuntimeType.FrameworkV4);
        }

        public override bool IsVisible(IMenuItemContext context) => Utils.IsDebugBuild;
    }
}
