using System;
using System.IO;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Msiler.AssemblyParser;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;

namespace Msiler.Helpers
{
    public static class DTEHelpers
    {
        private static readonly Regex GenericPartRegex
            = new Regex("<.*>", RegexOptions.Compiled);

        public static DTE2 GetDTE() {
            var provider = ServiceProvider.GlobalProvider;
            var vs = (DTE2)provider.GetService(typeof(DTE));

            if (vs == null) {
                throw new InvalidOperationException("DTE not found.");
            }
            return vs;
        }

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

        public static AssemblyMethodSignature GetSignature(VirtualPoint point, FileCodeModel2 fcm) {
            try {
                var me = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementFunction);

                var func = (CodeFunction)me;
                var funcName = func.FullName;
                // remove generic part
                funcName = GenericPartRegex.Replace(funcName, String.Empty);

                // make dnlib-compatible signature for constructors
                if (func.FunctionKind == vsCMFunction.vsCMFunctionConstructor) {
                    var lastIndex = funcName.LastIndexOf(func.Name, StringComparison.Ordinal);
                    funcName = funcName.Substring(0, lastIndex) + ".ctor";
                }
                var cfParams = func.Parameters;

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
                return new AssemblyMethodSignature(funcName, parameterList);
            } catch {
                return null;
            }
        }
    }
}
