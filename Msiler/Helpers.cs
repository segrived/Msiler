using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Quart.Msiler
{
    static class Helpers
    {
        public static string GetFullPath(string path, string basePath)
        {
            bool isAbsolute = Path.IsPathRooted(path);
            if (isAbsolute)
                return path;
            string saved = Environment.CurrentDirectory;
            Environment.CurrentDirectory = basePath;
            try {
                return Path.GetFullPath(path);
            } finally {
                Environment.CurrentDirectory = saved;
            }
        }

        public static DTE GetCurrentDocument()
        {
            var provider = ServiceProvider.GlobalProvider;
            var vs = (DTE)provider.GetService(typeof(DTE));
            if (vs == null)
                throw new InvalidOperationException("DTE not found.");
            return vs;
        }

        public static string GetOutputAssemblyFileName()
        {
            var dte = GetCurrentDocument();
            var prj = dte.ActiveDocument.ProjectItem.ContainingProject;
            string outFn = prj.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            string fullPath = GetFullPath(outFn, Path.GetDirectoryName(prj.FileName));
            return Path.Combine(fullPath, prj.Properties.Item("OutputFileName").Value.ToString());
        }
    }
}
