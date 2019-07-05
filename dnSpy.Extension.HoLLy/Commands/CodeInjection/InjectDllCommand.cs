using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.MVVM;
using HoLLy.dnSpyExtension.CodeInjection;
using HoLLy.dnSpyExtension.Dialogs;

namespace HoLLy.dnSpyExtension.Commands.CodeInjection
{
    [ExportMenuItem(Header = "Inject .NET DLL", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerInject)]
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
            if (!AskForEntryPoint(out MethodDef m))
                return;

            DbgProcess dbgProc = DbgManager.CurrentProcess.Current
                                 ?? DbgManager.Processes.FirstOrDefault()
                                 ?? throw new Exception("Couldn't find process");

            injector.Inject(dbgProc.Id, m, "Parameter", dbgProc.Bitness == 32);
        }

        public static bool AskForEntryPoint(out MethodDef method)
        {
            method = default;

            var ofd = new OpenFileDialog {
                Filter = PickFilenameConstants.DotNetAssemblyOrModuleFilter,
                Title = "Select .NET assembly to inject",
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return false;

            AssemblyDef asm;
            try {
                asm = AssemblyDef.Load(ofd.FileName);
            } catch (BadImageFormatException e) {
                MsgBox.Instance.Show("I couldn't load that binary. Are you sure it is a .NET assembly?\n" +
                                     "Reason: " + e.Message);
                return false;
            }

            var vm = new DLLEntryPointSelectionVM { Assembly = asm, };

            if (!vm.AllItems.Any()) {
                MsgBox.Instance.Show("Couldn't find any suitable entry points in that assembly.\n" +
                                     "Please make a method with the following signature: static int MethodName(string)");
                return false;
            }

            if (new DLLEntryPointSelection(vm).ShowDialog() != true)
                return false;

            method = vm.SelectedMethod;
            return true;
        }

        public override bool IsVisible(IMenuItemContext context) => DbgManager.IsDebugging;
        public override bool IsEnabled(IMenuItemContext context) => true;    // TODO: check if supported
    }
}
