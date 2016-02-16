using Msiler.HighlightSchemes;
using System.Collections.Generic;
using System.ComponentModel;

namespace Msiler.DialogPages
{
    public class ExtensionDisplayOptions : MsilerDialogPage
    {
        private const string CategoryTitle = "Display";

        [Category(CategoryTitle)]
        [DisplayName("Listing font name")]
        [Description("")]
        public string FontName { get; set; } = "Consolas";

        [Category(CategoryTitle)]
        [DisplayName("Listing font size")]
        [Description("")]
        public int FontSize { get; set; } = 12;

        [Category(CategoryTitle)]
        [DisplayName("Show line numbers")]
        [Description("")]
        public bool LineNumbers { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("VS Color theme")]
        [Description("Visual Studio color theme, Msiler highlighting will be adjusted based on this value")]
        public MsilerColorTheme ColorTheme { get; set; } = MsilerColorTheme.Auto;
    }
}
