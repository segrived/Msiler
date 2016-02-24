using System;
using System.IO;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Msiler.AssemblyParser;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;
using System.Linq;

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
            var codeFunction = GetCodeFunction(fcm, point);

            if (codeFunction == null) {
                return null;
            }
            // init and remove generic part
            string funcName = GenericPartRegex.Replace(codeFunction.FullName, String.Empty);
            IEnumerable<CodeTypeRef> paramsList = Enumerable.Empty<CodeTypeRef>();

            switch (codeFunction.FunctionKind) {
                case vsCMFunction.vsCMFunctionPropertyGet:
                case vsCMFunction.vsCMFunctionPropertySet:

                    var prefix = codeFunction.FunctionKind == vsCMFunction.vsCMFunctionPropertyGet
                        ? "get_"
                        : "set_";

                    var lastDot = funcName.LastIndexOf(".");
                    funcName = funcName.Substring(0, lastDot + 1) + prefix + funcName.Substring(lastDot + 1);

                    var funcParams = codeFunction.Parameters;
                    paramsList = codeFunction.FunctionKind == vsCMFunction.vsCMFunctionPropertyGet
                        ? new List<CodeTypeRef>()
                        : new List<CodeTypeRef> { codeFunction.Type }.OfType<CodeTypeRef>();
                    break;
                default:
                    if (codeFunction.FunctionKind == vsCMFunction.vsCMFunctionConstructor) {
                        var lastIndex = funcName.LastIndexOf(codeFunction.Name, StringComparison.Ordinal);
                        funcName = funcName.Substring(0, lastIndex) + ".ctor";
                    }
                    paramsList = codeFunction.Parameters.OfType<CodeParameter>().Select(p => p.Type);
                    break;
            }
            return new AssemblyMethodSignature(funcName, paramsList.Select(ProcessTypeRef).ToList());
        }

        private static string ProcessTypeRef(CodeTypeRef typeRef) {
            if (typeRef.TypeKind == vsCMTypeRef.vsCMTypeRefArray) {
                var rank = typeRef.Rank;
                var fullType = typeRef.ElementType.AsFullName + $"[{new String(',', rank - 1)}]";
                return fullType;
            } else {
                return typeRef.AsFullName;
            }
        }

        private static CodeFunction GetCodeFunction(FileCodeModel2 fcm, VirtualPoint point) {
            try {
                var element = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementFunction);
                return element as CodeFunction;
            } catch {
                return null;
            }
        }
    }
}
