using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Shell.Interop;

namespace Quart.Msiler
{
    public class Common
    {
        private static Common instance;

        private Common() { }

        public static Common Instance
        {
            get
            {
                if (instance == null) {
                    instance = new Common();
                }
                return instance;
            }
        }

        public IVsSolutionBuildManager Build { get; set; }
        public IVsWindowFrame Frame { get; set; }

        public MsilerPackage Package { get; set; }

        public ObservableCollection<string> Messages = new ObservableCollection<string>();
        public uint SolutionCookie { get; set; }
    }
}