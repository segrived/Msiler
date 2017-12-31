using System.ComponentModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Msiler.DialogPages
{
    public class ExtensionExcludeOptions : MsilerDialogPage
    {
        private const string CATEGORY_TITLE = "Exclude Methods";

        [Category(CATEGORY_TITLE)]
        [DisplayName("Exclude getters/setters")]
        [Description("")]
        public bool ExcludeProperties { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Exclude anonymous methods")]
        [Description("")]
        public bool ExcludeAnonymousMethods { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Exclude constructors")]
        [Description("")]
        public bool ExcludeConstructors { get; set; } = false;
    }
}
