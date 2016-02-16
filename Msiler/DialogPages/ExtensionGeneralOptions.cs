using System.ComponentModel;

namespace Msiler.DialogPages
{
    public class ExtensionGeneralOptions : MsilerDialogPage
    {
        private const string CategoryTitle = "General";

        [Category(CategoryTitle)]
        [DisplayName("Update listing only if toolbox is visible")]
        [Description("Bytecode listing  will be updated, only if Msiler toolbox visible on screen, it can reduce compilation times.")]
        public bool UpdateListingOnlyIfVisible { get; set; } = true;
    }

}
