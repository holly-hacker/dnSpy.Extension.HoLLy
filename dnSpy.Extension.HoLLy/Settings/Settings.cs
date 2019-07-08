using System.ComponentModel;
using System.ComponentModel.Composition;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.Settings;

namespace HoLLy.dnSpyExtension.Settings
{
    internal class Settings : ViewModelBase
    {
        private bool copyInjectedDLLToTemp;

        public bool CopyInjectedDLLToTemp
        {
            get => copyInjectedDLLToTemp;
            set {
                if (value != copyInjectedDLLToTemp) {
                    copyInjectedDLLToTemp = value;
                    OnPropertyChanged(nameof(CopyInjectedDLLToTemp));
                }
            }
        }

        public Settings Clone() => CopyTo(new Settings());

        public Settings CopyTo(Settings other)
        {
            other.CopyInjectedDLLToTemp = CopyInjectedDLLToTemp;
            return other;
        }
    }

    [Export(typeof(Settings))]
    internal class SettingsExportable : Settings
    {
        private readonly ISettingsService settingsService;

        [ImportingConstructor]
        public SettingsExportable(ISettingsService settingsService)
        {
            this.settingsService = settingsService;

            ISettingsSection sect = settingsService.GetOrCreateSection(Constants.SettingsGuid);
            CopyInjectedDLLToTemp = sect.Attribute<bool?>(nameof(CopyInjectedDLLToTemp)) ?? CopyInjectedDLLToTemp;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISettingsSection sect = settingsService.RecreateSection(Constants.SettingsGuid);
            sect.Attribute(nameof(CopyInjectedDLLToTemp), CopyInjectedDLLToTemp);
        }
    }
}
