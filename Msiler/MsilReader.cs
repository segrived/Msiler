using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Quart.Msiler
{
    public class MethodEntity
    {
        public MethodDefinition MethodData { get; set; }
        public List<Instruction> Instructions { get; set; }

        public string Name =>
            String.Format("{0}.{1}", MethodData.DeclaringType.FullName, MethodData.Name);

        public MethodEntity(MethodDefinition methodData, List<Instruction> instructions) {
            MethodData = methodData;
            Instructions = instructions;
        }
    }

    public class MsilReader
    {
        private readonly ModuleDefinition _module;
        private string AssemblyName { get; set; }

        public MsilReader(string assemblyName) {
            this.AssemblyName = assemblyName;
            this._module = ModuleDefinition.ReadModule(assemblyName);
        }

        //public IEnumerable<MethodEntity> FilterMaethods(IEnumerable<MethodEntity> methods) {
        //return methods.Where(m => m.MethodData.)
        //}

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
    }
}