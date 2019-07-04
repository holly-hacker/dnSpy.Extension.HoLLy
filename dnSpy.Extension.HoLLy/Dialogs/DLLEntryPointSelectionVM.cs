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
        public List<MethodDef> AllItems { get; private set; }

        public MethodDef SelectedMethod
        {
            get => selectedMethod;
            set {
                if (value != selectedMethod) {
                    selectedMethod = value;
                    OnPropertyChanged(nameof(SelectedMethod));
                }
            }
        }

        private MethodDef selectedMethod;

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
