using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;

namespace Msiler
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtensionOptions : DialogPage
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

        [Category("Listing generation options")]
        [DisplayName("Ignore NOPs")]
        [Description("If true, Nop instrutions will be excluded from listing")]
        public bool IgnoreNops { get; set; } = false;

        [Category("Listing generation options")]
        [DisplayName("Display numbers as HEX values")]
        [Description("")]
        public bool NumbersAsHex { get; set; } = false;

        [Category("Listing generation options")]
        [DisplayName("Simplify function names")]
        [Description("")]
        public bool SimplifyFunctionNames { get; set; } = false;

        [Category("Listing generation options")]
        [DisplayName("Upcase OpCodes")]
        [Description("If true, OpCodes will be upcased (for example LDSTR instead of ldstr)")]
        public bool UpcaseOpCodes { get; set; } = false;

        [Category("Listing generation options")]
        [DisplayName("Align listing")]
        [Description("")]
        public bool AlignListing { get; set; } = true;

        [Category("Listing generation options")]
        [DisplayName("Display offsets as decimal numbers")]
        [Description("")]
        public bool DecimalOffsets { get; set; } = false;

        [Category("Listing generation options")]
        [DisplayName("Display method names in listing")]
        [Description("")]
        public bool DisplayMethodNames { get; set; } = true;

        [Category("Global")]
        [DisplayName("Update listing only if toolbox is visible")]
        [Description("Bytecode listing  will be updated, only if Msiler toolbox visible on screen, it can reduce compilation times.")]
        public bool UpdateListingOnlyIfVisible { get; set; } = true;

        [Category("Global")]
        [DisplayName("Process PDB files")]
        [Description("NOTE: rebuilding required")]
        public bool ProcessPDBFiles { get; set; } = false;

        [Category("Global")]
        [DisplayName("Follow selected function in editor")]
        [Description("")]
        public bool FollowSelectedFunctionInEditor { get; set; } = false;


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

        protected override void OnApply(PageApplyEventArgs e) {
            OnApplied(e);
            base.OnApply(e);
        }

        // maybe it was bad idea
        public event ApplySettingsHandler Applied;

        protected virtual void OnApplied(EventArgs e) {
            if (Applied != null)
                Applied(this, e);
        }
    }

}
