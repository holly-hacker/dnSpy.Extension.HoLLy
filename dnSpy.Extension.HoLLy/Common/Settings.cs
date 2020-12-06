using System.Collections.Generic;
using System.Linq;
using dnSpy.Contracts.MVVM;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.Common
{
    internal class Settings : ViewModelBase
    {
        private const int MaxRecentInjections = 5;

        private bool underlineManagedAssemblies = true;
        private bool copyInjectedDllToTemp;
        private bool autoMapDllImports = true;
        private bool autoMapOverrides = true;
        private List<InjectionArguments> recentInjections = new List<InjectionArguments>();
        private string? diePath = string.Empty;

        public bool UnderlineManagedAssemblies
        {
            get => underlineManagedAssemblies;
            set {
                if (value != underlineManagedAssemblies) {
                    underlineManagedAssemblies = value;
                    OnPropertyChanged(nameof(UnderlineManagedAssemblies));
                }
            }
        }

        public bool CopyInjectedDllToTemp
        {
            get => copyInjectedDllToTemp;
            set {
                if (value != copyInjectedDllToTemp) {
                    copyInjectedDllToTemp = value;
                    OnPropertyChanged(nameof(CopyInjectedDllToTemp));
                }
            }
        }

        public bool AutoMapDllImports
        {
            get => autoMapDllImports;
            set {
                if (value != autoMapDllImports) {
                    autoMapDllImports = value;
                    OnPropertyChanged(nameof(AutoMapDllImports));
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

        public string? DiePath
        {
            get => diePath;
            set {
                if (value != diePath) {
                    diePath = value;
                    OnPropertyChanged(nameof(DiePath));
                }
            }
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
            other.UnderlineManagedAssemblies = UnderlineManagedAssemblies;
            other.CopyInjectedDllToTemp = CopyInjectedDllToTemp;
            other.AutoMapOverrides = AutoMapOverrides;
            other.AutoMapDllImports = AutoMapDllImports;
            other.RecentInjections = RecentInjections.ToList();
            other.DiePath = DiePath;
            return other;
        }
    }
}
