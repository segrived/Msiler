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
        public const string GuidMsilerPkgString = "2e50f4f0-18d1-419e-a204-f1156c910f2b";
        public const string GuidMsilerCmdSetString = "04d989fc-bbaa-4e42-aef8-c93d8727da2b";
        public const string GuidToolWindowPersistanceString = "0c127690-de92-4d02-a743-634bb922145c";
        public static readonly Guid GuidMsilerCmdSet = new Guid(GuidMsilerCmdSetString);
    };

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "2.1", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MsilerToolWindow), MultiInstances = false)]
    [ProvideOptionPage(typeof(ExtensionGeneralOptions), Common.OptionsGroupTitle, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(ExtensionDisplayOptions), Common.OptionsGroupTitle, "Display", 0, 0, true)]
    [ProvideOptionPage(typeof(ExtensionListingGenerationOptions), Common.OptionsGroupTitle, "Listing Generation", 0, 0, true)]
    [ProvideOptionPage(typeof(ExtensionExcludeOptions), Common.OptionsGroupTitle, "Exclude Methods Options", 0, 0, true)]
    [Guid(GuidList.GuidMsilerPkgString)]
    public sealed class MsilerPackage : Package
    {
        private IVsSolutionBuildManager _buildManager;
        private IVsSolution _solution;

        void ShowToolWindow(object sender, EventArgs e) {
            var window = this.FindToolWindow(typeof(MsilerToolWindow), 0, true);
            if (window?.Frame == null) {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize() {
            var menuCommandService = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == menuCommandService)
                return;

            var toolwndCommandId = new CommandID(GuidList.GuidMsilerCmdSet, (int)PkgCmdIDList.CmdidMyTool);
            var menuToolWin = new MenuCommand(this.ShowToolWindow, toolwndCommandId);
            menuCommandService.AddCommand(menuToolWin);

            this._buildManager = this.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            if (this._buildManager == null) {
                return;
            }

            this._solution = this.GetService(typeof(SVsSolution)) as IVsSolution;
            if (this._solution == null) {
                return;
            }

            Common.Instance.Package = this;
            Common.Instance.BuildManager = this._buildManager;
            Common.Instance.Solution = this._solution;

            Common.Instance.GeneralOptions
                = (ExtensionGeneralOptions)this.GetDialogPage(typeof(ExtensionGeneralOptions));
            Common.Instance.DisplayOptions
                = (ExtensionDisplayOptions)this.GetDialogPage(typeof(ExtensionDisplayOptions));
            Common.Instance.ListingGenerationOptions
                = (ExtensionListingGenerationOptions)this.GetDialogPage(typeof(ExtensionListingGenerationOptions));
            Common.Instance.ExcludeOptions
                = (ExtensionExcludeOptions)this.GetDialogPage(typeof(ExtensionExcludeOptions));
            base.Initialize();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (this._buildManager != null && Common.Instance.SolutionUpdateCookie != 0) {
                this._buildManager.UnadviseUpdateSolutionEvents(Common.Instance.SolutionUpdateCookie);
            }
            if (this._solution != null && Common.Instance.SolutionCookie != 0) {
                this._solution.UnadviseSolutionEvents(Common.Instance.SolutionCookie);
            }
        }
    }
}