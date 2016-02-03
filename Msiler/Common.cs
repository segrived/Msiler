using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Shell.Interop;

namespace Msiler
{
    public class Common
    {
        private static Common instance;
        public ObservableCollection<string> Messages = new ObservableCollection<string>();

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
        public IVsSolution SolutionManager { get; set; }
        public IVsWindowFrame Frame { get; set; }
        public MsilerPackage Package { get; set; }
        public uint SolutionUpdateCookie { get; set; }
        public uint SolutionCookie { get; set; }

        public MsilerOptions Options { get; set; }

        private Common() { }
    }
}