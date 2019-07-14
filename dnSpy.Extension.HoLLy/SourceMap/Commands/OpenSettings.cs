using System;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Settings.Dialog;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenuItem(Header = "Open settings...", Group = Constants.AppMenuGroupSourceMapSaveLoad, OwnerGuid = Constants.AppMenuGroupSourceMap, Order = 100)]
    internal class OpenSettings : MenuItemBase
    {
        private readonly Lazy<IAppSettingsService> appSettingsService;

        [ImportingConstructor]
        private OpenSettings(Lazy<IAppSettingsService> appSettingsService) => this.appSettingsService = appSettingsService;

        public override void Execute(IMenuItemContext context) => appSettingsService.Value.Show(Constants.SettingsPageGuid);
    }
}
