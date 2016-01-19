using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;

namespace Quart.Msiler
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MyToolWindow), MultiInstances = false)]
    [ProvideOptionPage(typeof(MsilerOptions), "Msiler", "Msiler", 0, 0, true)]
    [Guid(GuidList.guidMsilerPkgString)]
    public sealed class MsilerPackage : Package
    {
        private IVsSolutionBuildManager _buildManager;
        private IVsSolution _solutionManager;

        private void ShowToolWindow(object sender, EventArgs e) {
            var window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
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

    public class MsilerOptions : DialogPage
    {
        string fontName = "Consolas";
        int fontSize = 10;

        [Category("Display")]
        [DisplayName("Font name")]
        [Description("")]
        public string FontName {
            get { return fontName; }
            set { fontName = value; }
        }

        [Category("Display")]
        [DisplayName("Font size")]
        [Description("")]
        public int FontSize {
            get { return fontSize; }
            set { fontSize = value; }
        }
    }
}