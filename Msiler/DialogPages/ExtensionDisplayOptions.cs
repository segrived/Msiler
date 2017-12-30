using Msiler.Lib;
using System.ComponentModel;

// ReSharper disable MemberCanBePrivate.Global

namespace Msiler.DialogPages
{
    public class ExtensionDisplayOptions : MsilerDialogPage
    {
        private const string CATEGORY_TITLE = "Display";

        [Category(CATEGORY_TITLE)]
        [DisplayName("Listing font name")]
        [Description("")]
        public string FontName { get; set; } = "Consolas";

        [Category(CATEGORY_TITLE)]
        [DisplayName("Listing font size")]
        [Description("")]
        public int FontSize { get; set; } = 12;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Show line numbers")]
        [Description("")]
        public bool LineNumbers { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Tooltip transparency")]
        [Description("(In percent)")]
        public int TooltipTransparency { get; set; } = 0;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Color scheme")]
        [Description("")]
        public MsilerColorSchemeCode ColorScheme { get; set; } = MsilerColorSchemeCode.Auto;
    }
}
