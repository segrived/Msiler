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
        public List<AssemblyMethod> AllMethods { get; private set; }

        private readonly ModuleDefMD _module;

        public AssemblyReader(string assemblyFileName, AssemblyParserOptions options) {
            this.FileName = assemblyFileName;
            var creationOptions = new ModuleCreationOptions {
                TryToLoadPdbFromDisk = options.ProcessPDB
            };
            this._module = ModuleDefMD.Load(assemblyFileName, creationOptions);
            this.AllMethods = this.ParseMethodsList();
        }

        public AssemblyReader(string assemblyFileName) :
            this(assemblyFileName, new AssemblyParserOptions()) { }

        public List<AssemblyMethod> FilterMethods(bool exCtors = false, bool exProp = false, bool exAnon = false) {
            IEnumerable<AssemblyMethod> filteredMethods = AllMethods;
            if (exCtors)
                filteredMethods = filteredMethods.Where(m => !m.IsConstructor);
            if (exProp)
                filteredMethods = filteredMethods.Where(m => !m.IsProperty);
            if (exAnon)
                filteredMethods = filteredMethods.Where(m => !m.IsAnonymous);
            return filteredMethods.ToList();
        }

        private List<AssemblyMethod> ParseMethodsList() {
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