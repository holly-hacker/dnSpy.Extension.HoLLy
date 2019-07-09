using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using dnlib.DotNet;
using dnSpy.Contracts.MVVM;

namespace HoLLy.dnSpyExtension.Dialogs
{
    public class DLLEntryPointSelectionVM : ViewModelBase
    {
        public bool HasSelection => SelectedMethod != null;
        public AssemblyDef Assembly { set => ProcessAssembly(value); }
        public List<MethodDef> AllItems { get; private set; } = new List<MethodDef>();

        public MethodDef? SelectedMethod
        {
            get => selectedMethod;
            set {
                if (value != selectedMethod) {
                    selectedMethod = value;
                    OnPropertyChanged(nameof(SelectedMethod));
                }
            }
        }

        public string? Parameter
        {
            get => parameter;
            set {
                if (value != parameter) {
                    parameter = value;
                    OnPropertyChanged(nameof(Parameter));
                }
            }
        }

        private MethodDef? selectedMethod;
        private string? parameter;

        public DLLEntryPointSelectionVM()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedMethod))
                OnPropertyChanged(nameof(HasSelection));
        }

        private void ProcessAssembly(AssemblyDef assemblyDef)
        {
            AllItems = assemblyDef.ManifestModule
                .GetTypes()
                .SelectMany(t => t.Methods)
                .Where(m => m.IsStatic && isType<int>(m.ReturnType) && m.Parameters.Count == 1 && isType<string>(m.Parameters[0].Type))
                .ToList();

            static bool isType<T>(TypeSig t) => new SigComparer().Equals(t, typeof(T));
        }
    }
}
