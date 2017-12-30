using Microsoft.VisualStudio.Shell.Interop;
using Msiler.DialogPages;

namespace Msiler
{
    public class Common
    {
        private static Common _instance;

        public static Common Instance => _instance ?? (_instance = new Common());

        public const string OptionsGroupTitle = @"Msiler: MSIL Code Viewer";
        public const string RepoUrl = @"https://github.com/segrived/Msiler";
        public const string ReleaseNotesUrl = @"https://github.com/segrived/Msiler/wiki/Version-2.1-New-Features";

        public IVsSolutionBuildManager BuildManager { get; set; }
        public IVsSolution Solution { get; set; }
        public MsilerPackage Package { get; set; }
        public uint SolutionUpdateCookie { get; set; }
        public uint SolutionCookie { get; set; }

        #region Extension options pages
        public ExtensionGeneralOptions GeneralOptions { get; set; }
        public ExtensionDisplayOptions DisplayOptions { get; set; }
        public ExtensionListingGenerationOptions ListingGenerationOptions { get; set; }
        public ExtensionExcludeOptions ExcludeOptions { get; set; }
        #endregion

        private Common() { }
    }
}