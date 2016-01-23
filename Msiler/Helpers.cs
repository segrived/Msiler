using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using Quart.Msiler.Lib;

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

        public static DTE GetDTE() {
            var provider = ServiceProvider.GlobalProvider;
            var vs = (DTE)provider.GetService(typeof(DTE));

            if (vs == null) {
                throw new InvalidOperationException("DTE not found.");
            }
            return vs;
        }

        public static string GetOutputAssemblyFileName() {
            var dte = GetDTE();

            var sb = (SolutionBuild2)dte.Solution.SolutionBuild;
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

        public static bool IsFontFamilyExist(string fontFamily) {
            var fontsCollection = new InstalledFontCollection();
            return fontsCollection.Families.Any(ff => ff.Name == fontFamily);
        }

        public static string ReplaceNewLineCharacters(string str) {
            return str.Replace("\n", @"\n").Replace("\r", @"\r");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnonymous(this MethodEntity method) {
            var invalidChars = new[] { '<', '>' };
            return method.MethodName.Any(invalidChars.Contains);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsProperty(this MethodEntity method) {
            return method.MethodData.IsGetter || method.MethodData.IsSetter;
        }
    }
}