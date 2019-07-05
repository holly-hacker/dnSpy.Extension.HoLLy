
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
            var pid = MsgBox.Instance.Ask<int>("Process ID");
            var x86 = MsgBox.Instance.Ask<bool>("Is x86?");

            if (!InjectDllCommand.AskForEntryPoint(out MethodDef method))
                return;

            injector.Inject(pid, method, string.Empty, x86);
        }

        public override bool IsVisible(IMenuItemContext context) => Utils.IsDebugBuild;
    }
}
