using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Settings.Dialog;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.PluginSettings
{
    [Export(typeof(IAppSettingsPageProvider))]
    internal class SettingsPageProvider : IAppSettingsPageProvider
    {
        private readonly Settings settings;

        [ImportingConstructor]
        public SettingsPageProvider(Settings settings) => this.settings = settings;

        public IEnumerable<AppSettingsPage> Create()
        {
            yield return new SettingsPage(settings);
        }
    }

    internal class SettingsPage : AppSettingsPage, IAppSettingsPage2
    {
        private readonly Settings globalSettings;
        private readonly Settings newSettings;
        private SettingsControl? uiObject;

        public override Guid Guid => Constants.SettingsPageGuid;
        public override double Order => AppSettingsConstants.ORDER_BOOKMARKS + 10000;
        public override string Title => "dnSpy.Extension.HoLLy";
        public override object UIObject => uiObject ??= new SettingsControl { DataContext = newSettings };

        public SettingsPage(Settings settings)
        {
            globalSettings = settings;
            newSettings = settings.Clone();
        }

        public override void OnApply() => throw new NotSupportedException();

        public void OnApply(IAppRefreshSettings appRefreshSettings)
        {
            if (globalSettings.AutoMapOverrides != newSettings.AutoMapOverrides
                || globalSettings.AutoMapDLLImports != newSettings.AutoMapDLLImports)
                appRefreshSettings.Add(Constants.SourceMapSettingsChanged);

            newSettings.CopyTo(globalSettings);
        }
    }
}
