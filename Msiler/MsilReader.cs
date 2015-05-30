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
            get
            {
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
        public MethodDefinition MethodData { get; set; }
        public List<MsilInstruction> Instructions { get; set; }

        public string Name
        {
            get
            {
                return String.Format("{0}.{1}", MethodData.DeclaringType.FullName, MethodData.Name);
            }
        }

        public MsilMethodEntity(MethodDefinition methodData, List<MsilInstruction> instructions)
        {
            MethodData = methodData;
            Instructions = instructions;
        }
    }

    public class MsilReader
    {
        private readonly ModuleDefinition _module;
        private string AssemblyName { get; set; }

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
                where method.HasBody
                let instructions = body.Instructions.Select(i => new MsilInstruction(i))
                select new MsilMethodEntity(method, instructions.ToList());
        }
    }
}