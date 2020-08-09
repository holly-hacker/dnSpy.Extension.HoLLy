using System.Windows.Input;

namespace HoLLy.dnSpyExtension.CodeInjection.Dialogs
{
    internal partial class DllEntryPointSelection
    {
        public DllEntryPointSelection(DllEntryPointSelectionVm vm)
        {
            DataContext = vm;
            Title = "Select an entry point";
            InitializeComponent();
        }

        private void ListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!((DllEntryPointSelectionVm)DataContext).HasSelection)
                return;

            ClickOK();
        }
    }
}
