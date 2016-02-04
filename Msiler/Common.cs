using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Shell.Interop;
using Msiler.DialogPages;

namespace Msiler
{
    public class Common
    {
        static Common instance;

        public static Common Instance {
            get
            {
                if (instance == null) {
                    instance = new Common();
                }
                return instance;
            }
        }

        public IVsSolutionBuildManager BuildManager { get; set; }
        public IVsSolution Solution { get; set; }
        public IVsWindowFrame Frame { get; set; }
        public MsilerPackage Package { get; set; }
        public uint SolutionUpdateCookie { get; set; }
        public uint SolutionCookie { get; set; }

        #region Extension options pages
        public ExtensionGeneralOptions GeneralOptions { get; set; }
        public ExtensionDisplayOptions DisplayOptions { get; set; }
        public ExtensionListingGenerationOptions ListingGenerationOptions { get; set; }
        public ExtensionExcludeOptions ExcludeOptions { get; set; }
        #endregion

        Common() { }
    }
}