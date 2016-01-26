using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;

namespace Quart.Msiler
{
    internal static class GuidList
    {
        public const string guidMsilerPkgString = "2e50f4f0-18d1-419e-a204-f1156c910f2b";
        public const string guidMsilerCmdSetString = "04d989fc-bbaa-4e42-aef8-c93d8727da2b";
        public const string guidToolWindowPersistanceString = "0c127690-de92-4d02-a743-634bb922145c";
        public static readonly Guid guidMsilerCmdSet = new Guid(guidMsilerCmdSetString);
    };

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "2.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MsilerToolWindow), MultiInstances = false)]
    [ProvideOptionPage(typeof(MsilerOptions), "Msiler", "General", 0, 0, true)]
    [Guid(GuidList.guidMsilerPkgString)]
    public sealed class MsilerPackage : Package
    {
        private IVsSolutionBuildManager _buildManager;
        private IVsSolution _solutionManager;

        private void ShowToolWindow(object sender, EventArgs e) {
            var window = this.FindToolWindow(typeof(MsilerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame)) {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize() {
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) {
                return;
            }
            var toolwndCommandId = new CommandID(GuidList.guidMsilerCmdSet, (int)PkgCmdIDList.cmdidMyTool);
            var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
            mcs.AddCommand(menuToolWin);

            _buildManager = GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            if (_buildManager == null) {
                return;
            }

            _solutionManager = GetService(typeof(SVsSolution)) as IVsSolution;
            if (_solutionManager == null) {
                return;
            }

            Common.Instance.Package = this;
            Common.Instance.BuildManager = _buildManager;
            Common.Instance.SolutionManager = _solutionManager;
            Common.Instance.Options = (MsilerOptions)GetDialogPage(typeof(MsilerOptions));

            base.Initialize();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (_buildManager != null && Common.Instance.SolutionUpdateCookie != 0) {
                _buildManager.UnadviseUpdateSolutionEvents(Common.Instance.SolutionUpdateCookie);
            }
        }
    }

    public delegate void ApplySettingsHandler(object sender, EventArgs e);

    public enum MsilerColorTheme
    {
        Light, Dark, Auto
    }

    public class MsilerOptions : DialogPage
    {
        // Default values

        // display options
        string fontName = "Consolas";
        int fontSize = 12;
        bool lineNumbers = true;
        MsilerColorTheme colorTheme = MsilerColorTheme.Auto;

        // exclude options
        bool excludeAutoProperties = false;
        bool excludeAnonymousMethods = false;
        bool excludeConstructors = false;

        // listing generation options
        bool ignoreNops = false;
        bool numbersAsHex = false;
        bool simplifyFunctionNames = false;
        bool upcaseOpCodes = false;
        bool alignListing = false;
        bool decimalOffsets = false;
        bool methodNames = true;

        // global options
        bool updateListingOnlyIfVisible = true;

        // End of default Values

        // maybe it was bad idea
        public event ApplySettingsHandler Applied;

        protected virtual void OnApplied(EventArgs e) {
            if (Applied != null)
                Applied(this, e);
        }

        [Category("Display")]
        [DisplayName("Listing font name")]
        [Description("")]
        public string FontName {
            get { return fontName; }
            set { fontName = value; }
        }

        [Category("Display")]
        [DisplayName("Listing font size")]
        [Description("")]
        public int FontSize {
            get { return fontSize; }
            set { fontSize = value; }
        }

        [Category("Display")]
        [DisplayName("Show line numbers")]
        [Description("")]
        public bool LineNumbers {
            get { return lineNumbers; }
            set { lineNumbers = value; }
        }

        [Category("Display")]
        [DisplayName("VS Color theme")]
        [Description("Visual Studio color theme, Msiler highlighting will be adjusted based on this value")]
        public MsilerColorTheme ColorTheme {
            get { return colorTheme; }
            set { colorTheme = value; }
        }


        [Category("Listing generation options")]
        [DisplayName("Ignore NOPs")]
        [Description("If true, Nop instrutions will be excluded from listing")]
        public bool IgnoreNops {
            get { return ignoreNops; }
            set { ignoreNops = value; }
        }

        [Category("Listing generation options")]
        [DisplayName("Display numbers as HEX values")]
        [Description("")]
        public bool NumbersAsHex {
            get { return numbersAsHex; }
            set { numbersAsHex = value; }
        }

        [Category("Listing generation options")]
        [DisplayName("Simplify function names")]
        [Description("")]
        public bool SimplifyFunctionNames {
            get { return simplifyFunctionNames; }
            set { simplifyFunctionNames = value; }
        }

        [Category("Listing generation options")]
        [DisplayName("Upcase OpCodes")]
        [Description("If true, OpCodes will be upcased (for example LDSTR instead of ldstr)")]
        public bool UpcaseOpCodes {
            get { return upcaseOpCodes; }
            set { upcaseOpCodes = value; }
        }

        [Category("Listing generation options")]
        [DisplayName("Align listing")]
        [Description("")]
        public bool AlignListing {
            get { return alignListing; }
            set { alignListing = value; }
        }

        [Category("Listing generation options")]
        [DisplayName("Display offsets as decimal numbers")]
        [Description("")]
        public bool DecimalOffsets {
            get { return decimalOffsets; }
            set { decimalOffsets = value; }
        }

        [Category("Listing generation options")]
        [DisplayName("Display method names in listing")]
        [Description("")]
        public bool DisplayMethodNames {
            get { return methodNames; }
            set { methodNames = value; }
        }

        [Category("Global")]
        [DisplayName("Update listing only if toolbox is visible")]
        [Description("Bytecode listing  will be updated, only if Msiler toolbox visible on screen, it can reduce compilation times.")]
        public bool UpdateListingOnlyIfVisible {
            get { return updateListingOnlyIfVisible; }
            set { updateListingOnlyIfVisible = value; }
        }

        [Category("Excluded methods")]
        [DisplayName("Exclude getters/setters")]
        [Description("")]
        public bool ExcludeProperties {
            get { return excludeAutoProperties; }
            set { excludeAutoProperties = value; }
        }


        [Category("Excluded methods")]
        [DisplayName("Exclude anonymous methods")]
        [Description("")]
        public bool ExcludeAnonymousMethods {
            get { return excludeAnonymousMethods; }
            set { excludeAnonymousMethods = value; }
        }

        [Category("Excluded methods")]
        [DisplayName("Exclude constructors")]
        [Description("")]
        public bool ExcludeConstructors {
            get { return excludeConstructors; }
            set { excludeConstructors = value; }
        }

        protected override void OnApply(PageApplyEventArgs e) {
            OnApplied(e);
            base.OnApply(e);
        }
    }
}