using System;

namespace HoLLy.dnSpyExtension
{
    public static class Constants
    {
        public const string ContextMenuGroupEdit = "5050,HoLLy.ContextMenu.Edit";    // under "Edit <xxx>..."
        public const string ContextMenuGroupDebug = "-999,HoLLy.ContextMenu.Debug";
        public const string AppMenuGroupDebuggerDebug = "-999,HoLLy.AppMenu.Debugger.Debug";
        public const string AppMenuGroupDebuggerInject = "2500,HoLLy.AppMenu.Debugger.Inject";
        public const string AppMenuGroupDebuggerInjectRecent = "0,HoLLy.AppMenu.Debugger.InjectRecent";
        public const string AppMenuGuidDebuggerInjectRecent = "49AF3351-1A7D-444F-B7B4-F076C390E220";
        public const string AppMenuGroupSourceMap = "9320D902-D245-40DD-BF23-8887D0C83292";
        public const string AppMenuGroupSourceMapSaveLoad = "0,HoLLy.AppMenu.SourceMap.SaveLoad";

        public static readonly Guid DecompilerGuid = new Guid("590164A4-53F4-4677-B1E3-CC6500C273F6");
        public static readonly Guid SettingsGuid = new Guid("146D6180-E468-493F-BCBD-F778E761C58C");
        public static readonly Guid SettingsPageGuid = new Guid("4F026CFE-442B-4126-9FD7-E6469360A3A3");
    }
}
