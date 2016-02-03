using System;
using dnlib.DotNet;
using System.Collections.Generic;
using System.Linq;

namespace Msiler.AssemblyParser
{
    public class AssemblyParserOptions
    {
        public bool ProcessPDB { get; set; }
    }

    public class AssemblyReader : IDisposable
    {
        public string FileName { get; private set; }
        public List<AssemblyMethod> Methods { get; private set; }

        private readonly ModuleDefMD _module;

        public AssemblyReader(string assemblyFileName, AssemblyParserOptions options) {
            this.FileName = assemblyFileName;
            var creationOptions = new ModuleCreationOptions {
                TryToLoadPdbFromDisk = options.ProcessPDB
            };
            this._module = ModuleDefMD.Load(assemblyFileName, creationOptions);
            this.Methods = this.GetMethodsList();
        }

        public AssemblyReader(string assemblyFileName) :
            this(assemblyFileName, new AssemblyParserOptions()) { }

        private List<AssemblyMethod> GetMethodsList() {
            var moduleTypes = this._module.GetTypes();
            var assemblyMethodList = moduleTypes
                .SelectMany(t => t.Methods)
                .Where(m => m.HasBody)
                .Select(m => new AssemblyMethod(m));
            return assemblyMethodList.ToList();
        }

        public void Dispose() {
            this._module.Dispose();
        }
    }
}