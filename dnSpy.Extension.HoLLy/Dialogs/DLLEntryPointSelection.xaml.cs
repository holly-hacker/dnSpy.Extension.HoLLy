using System.Windows.Input;

namespace HoLLy.dnSpyExtension.Dialogs
{
    internal partial class DLLEntryPointSelection
    {
        public DLLEntryPointSelection(DLLEntryPointSelectionVM vm)
        {
            DataContext = vm;
            Title = "Select an entry point";
            InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!((DLLEntryPointSelectionVM)DataContext).HasSelection)
                return;

            ClickOK();
        }
    }
}
