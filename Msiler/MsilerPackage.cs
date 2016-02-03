using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;

namespace Msiler
{
    internal static class GuidList
    {
        public const string guidMsilerPkgString = "2e50f4f0-18d1-419e-a204-f1156c910f2b";
        public const string guidMsilerCmdSetString = "04d989fc-bbaa-4e42-aef8-c93d8727da2b";
        public const string guidToolWindowPersistanceString = "0c127690-de92-4d02-a743-634bb922145c";
        public static readonly Guid guidMsilerCmdSet = new Guid(guidMsilerCmdSetString);
    };

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "2.1-prealpha1", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MsilerToolWindow), MultiInstances = false)]
    [ProvideOptionPage(typeof(ExtenstionOptions), "Msiler", "General", 0, 0, true)]
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
            Common.Instance.Options = (ExtenstionOptions)GetDialogPage(typeof(ExtenstionOptions));

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
}