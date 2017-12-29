using System;
using dnlib.DotNet;
using System.Collections.Generic;
using System.Linq;

namespace Msiler.AssemblyParser
{
    public class AssemblyReader : IDisposable
    {
        public List<AssemblyMethod> Methods { get; }

        private readonly ModuleDefMD module;

        public AssemblyReader(string assemblyFileName, AssemblyParserOptions options)
        {
            var creationOptions = new ModuleCreationOptions
            {
                TryToLoadPdbFromDisk = options.ProcessPdb
            };
            this.module = ModuleDefMD.Load(assemblyFileName, creationOptions);
            this.Methods = this.ParseMethodsList();
        }

        private List<AssemblyMethod> ParseMethodsList()
        {
            var moduleTypes = this.module.GetTypes();

            var assemblyMethodList = moduleTypes
                .SelectMany(t => t.Methods)
                .Where(m => m.HasBody)
                .Select(m => new AssemblyMethod(m));

            return assemblyMethodList.ToList();
        }

        public void Dispose() => this.module.Dispose();
    }
}