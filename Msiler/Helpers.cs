using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using System.Drawing.Text;
using System.Linq;
using Msiler.AssemblyParser;
using System.Collections.Generic;

namespace Msiler
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

        public static DTE2 GetDTE() {
            var provider = ServiceProvider.GlobalProvider;
            var vs = (DTE2)provider.GetService(typeof(DTE));

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
            var activeConf = activeProject.ConfigurationManager.ActiveConfiguration;
            string outFn = activeConf.Properties.Item("OutputPath").Value.ToString();
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

        public static AssemblyMethodSignature GetSignature(this VirtualPoint point, FileCodeModel2 fcm) {
            try {
                var ne = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementNamespace);
                var ce = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementClass);
                var me = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementFunction);

                // should be compitable with Mono.Cecil method names
                string fnName = (ce.Name == me.Name) ? ".ctor" : me.Name;

                var cfParams = ((CodeFunction)me).Parameters;
                var parameterList = new List<string>();
                foreach (CodeParameter param in cfParams) {
                    if (param.Type.TypeKind == vsCMTypeRef.vsCMTypeRefArray) {
                        var arrayType = param.Type.ElementType;
                        var fullType = arrayType.AsFullName + "[]";
                        parameterList.Add(fullType);
                    } else {
                        parameterList.Add(param.Type.AsFullName);
                    }
                }
                return new AssemblyMethodSignature($"{ne.Name}.{ce.Name}.{fnName}", parameterList);
            } catch {
                return null;
            }
        }
    }
}