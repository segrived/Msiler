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
        private int _longestOpCode;

        private MsilerOptions _options;

        public ListingGenerator() {
            this._options = Common.Instance.Options;
        }

        private string GetOffset(Instruction i) =>
            String.Format("IL_{0:X4}", i.Offset);

        private string GetOperand(Instruction i) {
            if (i.OpCode.Code == Code.Ldstr) {
                return @"""" + Helpers.ReplaceNewLineCharacters(i.Operand.ToString()) + @"""";
            }

            if (i.Operand == null) {
                return String.Empty;
            }

            if (i.Operand is Instruction) {
                return GetOffset((Instruction)i.Operand);
            }

            if (i.Operand is Instruction[]) {
                var operands = (Instruction[])i.Operand;
                var joined = String.Join(" | ", operands.Select(GetOffset));
                return $"[ {joined} ]";
            }

            if (this._options.SimplifyFunctionNames && (i.Operand is MethodReference)) {
                var m = (MethodReference)i.Operand;
                return $"{m.DeclaringType.FullName}.{m.Name}";
            }

            if (this._options.NumbersAsHex) {
                Int64 number;
                bool isNumeric = Int64.TryParse(i.Operand.ToString(), out number);
                if (isNumeric) {
                    return $"0x{number.ToString("X")}";
                }
            }

            return i.Operand.ToString();
        }

        public string GetOpCode(Instruction i) {
            var name = i.OpCode.Name;
            return (this._options.UpcasedInstructionNames) ? name.ToUpper() : name;
        }

        private string InstructionToString(Instruction i, int longestOpcode) {
            var opcodePart = this._options.AlignListing
                ? GetOpCode(i).PadRight(longestOpcode + 1)
                : GetOpCode(i);
            return $"{GetOffset(i)} {opcodePart} {GetOperand(i)}";
        }

        public string Generate(IEnumerable<Instruction> instructions) {
            if (this._options.AlignListing) {
                this._longestOpCode = instructions
                    .Select(i => i.OpCode.Name)
                    .Max(s => s.Length);
            }
            var sb = new StringBuilder();
            foreach (var instruction in instructions) {
                if (this._options.IgnoreNops && instruction.OpCode.Code == Code.Nop) {
                    continue;
                }
                sb.AppendLine(InstructionToString(instruction, _longestOpCode));
            }
            return sb.ToString();
        }
    }
}
