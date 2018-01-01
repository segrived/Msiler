using Msiler.DialogPages;
using System.Windows.Controls;

namespace Msiler.UI.UserControls
{
    public partial class WelcomeUserControl : UserControl
    {
        public WelcomeUserControl() {
            this.InitializeComponent();
        }

        private void HyperlinkOptions_Click(object sender, System.Windows.RoutedEventArgs e) 
            => Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));

        private void HyperlinkGithub_Click(object sender, System.Windows.RoutedEventArgs e) 
            => System.Diagnostics.Process.Start(Common.RepoUrl);

        private void HyperlinkInstructions_Click(object sender, System.Windows.RoutedEventArgs e) 
            => new ILHelpWindow().ShowDialog();
    }
}
