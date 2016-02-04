using System.ComponentModel;

namespace Msiler.DialogPages
{
    public class ExtensionDisplayOptions : MsilerDialogPage
    {
        [Category("Display")]
        [DisplayName("Listing font name")]
        [Description("")]
        public string FontName { get; set; } = "Consolas";

        [Category("Display")]
        [DisplayName("Listing font size")]
        [Description("")]
        public int FontSize { get; set; } = 12;

        [Category("Display")]
        [DisplayName("Show line numbers")]
        [Description("")]
        public bool LineNumbers { get; set; } = true;

        [Category("Display")]
        [DisplayName("VS Color theme")]
        [Description("Visual Studio color theme, Msiler highlighting will be adjusted based on this value")]
        public MsilerColorTheme ColorTheme { get; set; } = MsilerColorTheme.Auto;
    }
}
