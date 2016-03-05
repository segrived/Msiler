using System.Windows;
using System.Diagnostics;
using Msiler.Lib;

namespace Msiler.UI
{
    public partial class AboutWindow
    {
        public AboutWindow() {
            this.InitializeComponent();

            var manifest = VsixManifest.GetManifest();
            this.VersionTextBlock.Text = $"Version {manifest.Version}";
        }

        void Ok_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        void Hyperlink_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://github.com/segrived/Msiler");
        }
    }
}
