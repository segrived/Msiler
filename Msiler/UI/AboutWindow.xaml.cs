using System.Windows;
using System.Diagnostics;

namespace Msiler.UI
{
    public partial class AboutWindow : Window
    {
        private readonly AboutWindowVM _viewModel;

        public AboutWindow() {
            InitializeComponent();
            this._viewModel = new AboutWindowVM();
            this.DataContext = this._viewModel;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://github.com/segrived/Msiler");
        }
    }
}
