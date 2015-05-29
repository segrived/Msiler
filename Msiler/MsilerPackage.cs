using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Quart.Msiler
{

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MyToolWindow), MultiInstances = false)]
    [Guid(GuidList.guidMsilerPkgString)]
    public sealed class MsilerPackage : Package
    {
        private IVsSolutionBuildManager _buildManager;

        private void ShowToolWindow(object sender, EventArgs e)
        {
            var window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame)) {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        protected override void Initialize()
        {
            base.Initialize();

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

            Common.Instance.Package = this;
            Common.Instance.Build = _buildManager;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_buildManager != null && Common.Instance.SolutionCookie != 0) {
                _buildManager.UnadviseUpdateSolutionEvents(Common.Instance.SolutionCookie);
            }
        }
    }
}
