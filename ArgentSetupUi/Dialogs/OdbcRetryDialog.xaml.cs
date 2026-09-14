using System.Windows;

namespace Argent.SetupUi.Dialogs
{
    public partial class OdbcRetryDialog : Window
    {
        public bool RetrySelected { get; private set; }

        public OdbcRetryDialog()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            RetrySelected = true;
            DialogResult = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            RetrySelected = false;
            DialogResult = true;
            Close();
        }
    }
}
