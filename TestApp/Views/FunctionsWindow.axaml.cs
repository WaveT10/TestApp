using Avalonia.Controls;

namespace TestApp
{
    public partial class FunctionsWindow : Window
    {
        public FunctionsWindow()
        {
            InitializeComponent();
        }

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            if (!e.IsProgrammatic && DataContext is FunctionsViewModel functions)
            {
                e.Cancel = true;

                var canProceed = await functions.ProceedWithSavingFunctions(StorageProvider);

                if (canProceed) 
                {
                    Close();
                }
            }

            base.OnClosing(e);
        }
    }
}