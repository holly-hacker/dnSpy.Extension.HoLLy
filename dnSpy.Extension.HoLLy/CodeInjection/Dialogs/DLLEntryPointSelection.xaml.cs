using System.Windows.Input;

namespace HoLLy.dnSpyExtension.CodeInjection.Dialogs
{
    internal partial class DLLEntryPointSelection
    {
        public DLLEntryPointSelection(DLLEntryPointSelectionVM vm)
        {
            DataContext = vm;
            Title = "Select an entry point";
            InitializeComponent();
        }

        private void ListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!((DLLEntryPointSelectionVM)DataContext).HasSelection)
                return;

            ClickOK();
        }
    }
}
