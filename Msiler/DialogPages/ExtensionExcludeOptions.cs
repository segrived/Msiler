using System.ComponentModel;

// ReSharper disable MemberCanBePrivate.Global

namespace Msiler.DialogPages
{
    public class ExtensionExcludeOptions : MsilerDialogPage
    {
        private const string CategoryTitle = "Exclude Methods";

        [Category(CategoryTitle)]
        [DisplayName("Exclude getters/setters")]
        [Description("")]
        public bool ExcludeProperties { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Exclude anonymous methods")]
        [Description("")]
        public bool ExcludeAnonymousMethods { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Exclude constructors")]
        [Description("")]
        public bool ExcludeConstructors { get; set; } = false;
    }
}
