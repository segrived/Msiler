using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Msiler.DialogPages;

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
    [InstalledProductRegistration("#110", "#112", "2.1-beta1", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MsilerToolWindow), MultiInstances = false)]
    [ProvideOptionPage(typeof(ExtensionGeneralOptions), "Msiler", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(ExtensionDisplayOptions), "Msiler", "Display", 0, 0, true)]
    [ProvideOptionPage(typeof(ExtensionListingGenerationOptions), "Msiler", "Listing Generation", 0, 0, true)]
    [ProvideOptionPage(typeof(ExtensionExcludeOptions), "Msiler", "Exclude Methods Options", 0, 0, true)]
    [Guid(GuidList.guidMsilerPkgString)]
    public sealed class MsilerPackage : Package
    {
        IVsSolutionBuildManager _buildManager;
        IVsSolution _solution;

        void ShowToolWindow(object sender, EventArgs e) {
            var window = this.FindToolWindow(typeof(MsilerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame)) {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize() {
            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == menuCommandService)
                return;

            var toolwndCommandId = new CommandID(GuidList.guidMsilerCmdSet, (int)PkgCmdIDList.cmdidMyTool);
            var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
            menuCommandService.AddCommand(menuToolWin);

            this._buildManager = GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            if (_buildManager == null)
                return;

            this._solution = GetService(typeof(SVsSolution)) as IVsSolution;
            if (_solution == null)
                return;

            Common.Instance.Package = this;
            Common.Instance.BuildManager = this._buildManager;
            Common.Instance.Solution = this._solution;

            Common.Instance.GeneralOptions
                = (ExtensionGeneralOptions)GetDialogPage(typeof(ExtensionGeneralOptions));
            Common.Instance.DisplayOptions
                = (ExtensionDisplayOptions)GetDialogPage(typeof(ExtensionDisplayOptions));
            Common.Instance.ListingGenerationOptions
                = (ExtensionListingGenerationOptions)GetDialogPage(typeof(ExtensionListingGenerationOptions));
            Common.Instance.ExcludeOptions
                = (ExtensionExcludeOptions)GetDialogPage(typeof(ExtensionExcludeOptions));
            base.Initialize();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (_buildManager != null && Common.Instance.SolutionUpdateCookie != 0) {
                _buildManager.UnadviseUpdateSolutionEvents(Common.Instance.SolutionUpdateCookie);
            }
            if (this._solution != null && Common.Instance.SolutionCookie != 0) {
                _solution.UnadviseSolutionEvents(Common.Instance.SolutionCookie);
            }
        }
    }

    public enum MsilerColorTheme
    {
        Light, Dark, Auto
    }
}