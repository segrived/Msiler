using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Quart.Msiler
{
    public class ListingGenerator
    {
        public bool IgnoreNops { get; set; }
        public bool NumbersAsHex { get; set; }
        public bool SimplifyFunctionNames { get; set; }
        public bool UpcaseInstructionNames { get; set; }
        public bool AlignListing { get; set; }

        private int _longestOpCode;

        private string GetOffset(Instruction i) =>
            String.Format("IL_{0:X4}", i.Offset);

        private string GetOperand(Instruction i) {
            if (i.OpCode.Code == Code.Ldstr) {
                return @"""" + i.Operand.ToString() + @"""";
            }

            if (i.Operand == null) {
                return String.Empty;
            }

            if (i.Operand is Instruction) {
                return GetOffset((Instruction)i.Operand);
            }

            if (i.Operand is Instruction[]) {
                var operands = (Instruction[])i.Operand;
                return String.Join(" | ", operands.Select(GetOffset));
            }

            if (this.SimplifyFunctionNames && (i.Operand is MethodReference)) {
                var m = (MethodReference)i.Operand;
                return $"{m.DeclaringType.FullName}.{m.Name}";
            }

            if (this.NumbersAsHex) {
                Int64 number;
                bool isNumeric = Int64.TryParse(i.Operand.ToString(), out number);
                if (isNumeric)
                    return number.ToString("X");
            }

            return i.Operand.ToString();
        }

        public string GetOpCode(Instruction i) {
            var name = i.OpCode.Name;
            return (this.UpcaseInstructionNames) ? name.ToUpper() : name;
        }

        private string InstructionToString(Instruction i, int longestOpcode) {
            if (this.AlignListing) {
                return $"{GetOffset(i)} {GetOpCode(i).PadRight(longestOpcode + 1)} {GetOperand(i)}";
            } else {
                return $"{GetOffset(i)} {GetOpCode(i)} {GetOperand(i)}";
            }

        }

        public string Generate(IEnumerable<Instruction> instructions) {
            if (this.AlignListing) {
                this._longestOpCode = instructions
                    .Select(i => i.OpCode.Name)
                    .Max(s => s.Length);
            }
            var sb = new StringBuilder();
            foreach (var instruction in instructions) {
                if (this.IgnoreNops && instruction.OpCode.Code == Code.Nop) {
                    continue;
                }
                sb.AppendLine(InstructionToString(instruction, _longestOpCode));
            }
            return sb.ToString();
        }
    }
}
