using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quart.Msiler.Lib
{
    public class MethodEntity
    {
        public MethodDefinition MethodData { get; set; }
        public List<Instruction> Instructions { get; set; }

        public string MethodName =>
            $"{MethodData.DeclaringType.FullName}.{MethodData.Name}";


        public string Parameters =>
            String.Join(", ", MethodData.Parameters.Select(p => p.ParameterType.FullName));

        public MethodEntity(MethodDefinition methodData, List<Instruction> instructions) {
            MethodData = methodData;
            Instructions = instructions;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var objEntity = (MethodEntity)obj;
            return this.MethodData.FullName == objEntity.MethodData.FullName;
        }

        public override int GetHashCode() {
            return this.MethodData.FullName.GetHashCode();
        }
    }
}
