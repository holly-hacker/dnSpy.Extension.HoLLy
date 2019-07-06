using System;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Menus;

namespace HoLLy.dnSpyExtension.Commands.CodeInjection.Debug
{
    [ExportMenuItem(Header = "DBG: Show Process Info", OwnerGuid = MenuConstants.APP_MENU_DEBUG_GUID, Group = Constants.AppMenuGroupDebuggerDebug)]
    internal class ShowProcessInfo : MenuItemBase
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private readonly Lazy<DbgManager> dbgManagerLazy;

        [ImportingConstructor]
        public ShowProcessInfo(Lazy<DbgManager> dbgManager)
        {
            dbgManagerLazy = dbgManager;
        }

        public override void Execute(IMenuItemContext context)
        {
            var proc = DbgManager.CurrentProcess.Current
                       ?? DbgManager.Processes.FirstOrDefault()
                       ?? throw new Exception("Couldn't find process");

            MsgBox.Instance.Show($"Process ID: {proc.Id} (0x{proc.Id:x})\n" +
                                 $"Architecture: {proc.Architecture} ({proc.Bitness}bit, {proc.PointerSize} byte pointers)\n" +
                                 $"Runtime: {proc.Runtimes[0].Name}\n" +
                                 $"Runtime ID: {proc.Runtimes[0].Id}\n" +
                                 $"Runtime Tags: {string.Join(", ", proc.Runtimes[0].Tags)}\n" +
                                 $"File name: {proc.Filename}");
        }

        public override bool IsVisible(IMenuItemContext context) => Utils.IsDebugBuild && DbgManager.IsDebugging;
        public override bool IsEnabled(IMenuItemContext context) => DbgManager.Processes.Any();
    }
}
