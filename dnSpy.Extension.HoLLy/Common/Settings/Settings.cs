using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.Settings;
using HoLLy.dnSpyExtension.CodeInjection;

namespace HoLLy.dnSpyExtension.Common.Settings
{
    internal class Settings : ViewModelBase
    {
        private const int MaxRecentInjections = 5;

        private bool copyInjectedDLLToTemp;
        private List<InjectionArguments> recentInjections = new List<InjectionArguments>();

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

        public IReadOnlyList<InjectionArguments> RecentInjections
        {
            get => recentInjections;
            protected set => recentInjections = (List<InjectionArguments>)value;
        }

        public void AddRecentInjection(InjectionArguments injectionArguments)
        {
            while (recentInjections.Contains(injectionArguments))
                recentInjections.Remove(injectionArguments);

            recentInjections.Insert(0, injectionArguments);

            if (recentInjections.Count > MaxRecentInjections)
                recentInjections.RemoveRange(MaxRecentInjections, recentInjections.Count - MaxRecentInjections);

            OnPropertyChanged(nameof(RecentInjections));
        }

        public Settings Clone() => CopyTo(new Settings());

        public Settings CopyTo(Settings other)
        {
            other.CopyInjectedDLLToTemp = CopyInjectedDLLToTemp;
            other.RecentInjections = RecentInjections.ToList();
            return other;
        }
    }

    [Export(typeof(Settings))]
    internal class SettingsExportable : Settings
    {
        private readonly ISettingsService settingsService;
        private const string SectionRecentInjections = "RecentInjections";
        private const string SectionRecentInjectionsInjection = "Injection";

        [ImportingConstructor]
        public SettingsExportable(ISettingsService settingsService)
        {
            this.settingsService = settingsService;

            ISettingsSection sect = settingsService.GetOrCreateSection(Constants.SettingsGuid);
            CopyInjectedDLLToTemp = sect.Attribute<bool?>(nameof(CopyInjectedDLLToTemp)) ?? CopyInjectedDLLToTemp;

            var sectInjections = sect.TryGetSection(SectionRecentInjections);
            if (!(sectInjections is null))
                RecentInjections = sectInjections.SectionsWithName(SectionRecentInjectionsInjection).Select(InjectionArguments.FromSection).ToList();

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISettingsSection sect = settingsService.RecreateSection(Constants.SettingsGuid);
            sect.Attribute(nameof(CopyInjectedDLLToTemp), CopyInjectedDLLToTemp);

            ISettingsSection sectInjections = sect.GetOrCreateSection(SectionRecentInjections);
            foreach (var injection in RecentInjections) {
                ISettingsSection sectInjection = sectInjections.CreateSection(SectionRecentInjectionsInjection);
                sectInjection.Attribute(nameof(injection.Path), injection.Path);
                sectInjection.Attribute(nameof(injection.Type), injection.Type);
                sectInjection.Attribute(nameof(injection.Method), injection.Method);
                if (injection.Argument != null) sectInjection.Attribute(nameof(injection.Argument), injection.Argument);
            }
        }
    }
}
