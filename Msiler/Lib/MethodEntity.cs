using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Quart.Msiler.Lib
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
}
