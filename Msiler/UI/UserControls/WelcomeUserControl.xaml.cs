using Msiler.DialogPages;
using System.Windows.Controls;

namespace Msiler.UI.UserControls
{
    public partial class WelcomeUserControl : UserControl
    {
        public WelcomeUserControl() {
            InitializeComponent();
        }

        private void HyperlinkOptions_Click(object sender, System.Windows.RoutedEventArgs e) {
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));
        }

        private void HyperlinkGithub_Click(object sender, System.Windows.RoutedEventArgs e) {
            System.Diagnostics.Process.Start(Common.RepoUrl);
        }

        private void HyperlinkAbout_Click(object sender, System.Windows.RoutedEventArgs e) {
            var win = new AboutWindow();
            win.ShowDialog();
        }
    }
}
