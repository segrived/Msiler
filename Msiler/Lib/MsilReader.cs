using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using System;

namespace Quart.Msiler.Lib
{
    public class MsilReader : IDisposable
    {
        private readonly ModuleDefMD _module;
        private string AssemblyName { get; set; }

        public MsilReader(string assemblyName, bool processSymbols) {
            this.AssemblyName = assemblyName;
            var creationOptions = new ModuleCreationOptions {
                TryToLoadPdbFromDisk = processSymbols,
                PdbImplementation = dnlib.DotNet.Pdb.PdbImplType.Default
            };
            this._module = ModuleDefMD.Load(assemblyName, creationOptions);
        }

        public IEnumerable<MethodEntity> EnumerateMethods() {
            var types = this._module.GetTypes();
            return
                from type in types
                from method in type.Methods
                let body = method.Body
                where method.HasBody
                let instructions = body.Instructions
                select new MethodEntity(method, instructions.ToList());
        }

        public void Dispose() {
            this._module.Dispose();
        }
    }
}
