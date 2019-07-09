using System.ComponentModel.Composition;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.CodeInjection.Commands.Debug
{
    [ExportMenuItem(Header = "DBG: Inject DLL", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerDebug)]
    internal class InjectDllDebugCommand : MenuItemBase
    {
        private readonly IManagedInjector injector;

        [ImportingConstructor]
        public InjectDllDebugCommand(IManagedInjector injector) => this.injector = injector;

        public override void Execute(IMenuItemContext context)
        {
            var pid = MsgBox.Instance.Ask<int?>("Process ID");
            if (pid is null) return;

            var x86 = MsgBox.Instance.Ask<bool?>("Is x86?");
            if (x86 is null) return;

            if (!InjectDllCommand.AskForEntryPoint(out InjectionArguments args))
                return;

            injector.Inject(pid.Value, args, x86.Value, RuntimeType.FrameworkV4);
        }

        public override bool IsVisible(IMenuItemContext context) => Utils.IsDebugBuild;
    }
}
