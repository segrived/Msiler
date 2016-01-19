using System;
using System.IO;
using System.Security.Cryptography;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Reflection;
using EnvDTE80;
using System.Drawing.Text;
using System.Linq;

namespace Quart.Msiler
{
    internal static class Helpers
    {
        public static string GetFullPath(string path, string basePath) {
            bool isAbsolute = Path.IsPathRooted(path);
            if (isAbsolute) {
                return path;
            }
            string saved = Environment.CurrentDirectory;
            Environment.CurrentDirectory = basePath;
            try {
                return Path.GetFullPath(path);
            } finally {
                Environment.CurrentDirectory = saved;
            }
        }

        public static DTE GetCurrentDocument() {
            var provider = ServiceProvider.GlobalProvider;
            var vs = (DTE)provider.GetService(typeof(DTE));

            if (vs == null) {
                throw new InvalidOperationException("DTE not found.");
            }
            return vs;
        }

        public static string GetOutputAssemblyFileName() {
            var dte = GetCurrentDocument();

            SolutionBuild2 sb = (SolutionBuild2)dte.Solution.SolutionBuild;
            var projects = sb.StartupProjects as Array;
            var activeProject = dte.Solution.Item(projects.GetValue(0));

            string outFn =
                activeProject.ConfigurationManager
                    .ActiveConfiguration
                    .Properties
                    .Item("OutputPath")
                    .Value
                    .ToString();
            string fullPath = GetFullPath(outFn, Path.GetDirectoryName(activeProject.FileName));
            return Path.Combine(fullPath, activeProject.Properties.Item("OutputFileName").Value.ToString());
        }

        public static byte[] ComputeMd5(string fn) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(fn)) {
                    return md5.ComputeHash(stream);
                }
            }
        }

        public static IHighlightingDefinition GetILHighlightingDefinition() {
            var ilRes = "Quart.Msiler.Resources.IL.xshd";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ilRes)) {
                using (var reader = new System.Xml.XmlTextReader(stream)) {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        public static bool IsFontFamilyExist(string fontFamily) {
            var fontsCollection = new InstalledFontCollection();
            return fontsCollection.Families.Any(ff => ff.Name == fontFamily);
        }

        public static string ReplaceNewLineCharacters(string str) {
            return str.Replace("\n", @"\n").Replace("\r", @"\r");
        }
    }
}