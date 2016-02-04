using System.ComponentModel;

namespace Msiler.DialogPages
{
    public class ExtensionExcludeOptions : MsilerDialogPage
    {
        [Category("Excluded methods")]
        [DisplayName("Exclude getters/setters")]
        [Description("")]
        public bool ExcludeProperties { get; set; } = false;

        [Category("Excluded methods")]
        [DisplayName("Exclude anonymous methods")]
        [Description("")]
        public bool ExcludeAnonymousMethods { get; set; } = false;

        [Category("Excluded methods")]
        [DisplayName("Exclude constructors")]
        [Description("")]
        public bool ExcludeConstructors { get; set; } = false;
    }
}
