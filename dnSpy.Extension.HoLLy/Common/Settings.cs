using System.Collections.Generic;
using System.Linq;
using dnSpy.Contracts.MVVM;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.Common
{
    internal class Settings : ViewModelBase
    {
        private const int MaxRecentInjections = 5;

        private bool copyInjectedDLLToTemp;
        private bool autoMapDLLImports = true;
        private bool autoMapOverrides = true;
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

        public bool AutoMapDLLImports
        {
            get => autoMapDLLImports;
            set {
                if (value != autoMapDLLImports) {
                    autoMapDLLImports = value;
                    OnPropertyChanged(nameof(AutoMapDLLImports));
                }
            }
        }

        public bool AutoMapOverrides
        {
            get => autoMapOverrides;
            set {
                if (value != autoMapOverrides) {
                    autoMapOverrides = value;
                    OnPropertyChanged(nameof(AutoMapOverrides));
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
}
