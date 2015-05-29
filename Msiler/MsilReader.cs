using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Quart.Msiler
{
    public class MsilInstruction
    {
        private Instruction Instruction { get; set; }

        public string Offset
        {
            get { return String.Format("IL_{0:X4}", Instruction.Offset); }
        }

        public OpCode OpCode
        {
            get { return Instruction.OpCode; }
        }

        public string Description
        {
            get {
                return
                    MsilInstructionsDescription.InstructionDescriptions.ContainsKey(this.OpCode.Name)
                        ? MsilInstructionsDescription.InstructionDescriptions[this.OpCode.Name]
                        : String.Empty;
            }
        }

        public object Operand
        {
            get
            {
                if (Instruction.Operand is Instruction) {
                    return new MsilInstruction((Instruction)Instruction.Operand).Offset;
                }
                return Instruction.Operand;
            }
        }

        public MsilInstruction(Instruction instruction)
        {
            this.Instruction = instruction;
        }
    }

    public class MsilMethodEntity
    {
        public MsilMethodEntity(MethodDefinition methodData, List<MsilInstruction> instructions)
        {
            MethodData = methodData;
            Instructions = instructions;
        }

        public MethodDefinition MethodData { get; set; }
        public List<MsilInstruction> Instructions { get; set; }
    }

    public class MsilReader
    {
        private string AssemblyName { get; set; }
        private readonly ModuleDefinition _module;

        public MsilReader(string assemblyName)
        {
            this.AssemblyName = assemblyName;
            this._module = ModuleDefinition.ReadModule(assemblyName);
        }

        public IEnumerable<MsilMethodEntity> EnumerateMethods()
        {
            var types = this._module.GetTypes();
            return 
                from type in types 
                from method in type.Methods 
                let body = method.Body 
                where body != null 
                let instructions = body.Instructions.Select(i => new MsilInstruction(i)) 
                select new MsilMethodEntity(method, instructions.ToList());
        }
    }
}
