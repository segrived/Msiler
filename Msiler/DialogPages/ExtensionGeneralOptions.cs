using System.ComponentModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Msiler.DialogPages
{
    public class ExtensionGeneralOptions : MsilerDialogPage
    {
        private const string CATEGORY_TITLE = "General";

        [Category(CATEGORY_TITLE)]
        [DisplayName("Update listing only if toolbox is visible")]
        [Description("Bytecode listing  will be updated, only if Msiler toolbox visible on screen, it can reduce compilation times.")]
        public bool UpdateListingOnlyIfVisible { get; set; } = true;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Enable listing caching")]
        [Description("Enabled by default, can be disabled for debugging purposes")]
        public bool EnableCaching { get; set; } = true;
    }

}
