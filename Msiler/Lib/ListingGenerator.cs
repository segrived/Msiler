using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.IO;

namespace Quart.Msiler.Lib
{
    public class ListingGenerator
    {
        private int _longestOpCode;

        private readonly MsilerOptions _options;

        private Dictionary<string, List<string>> _pdbCache =
            new Dictionary<string, List<string>>();

        public ListingGenerator() {
            this._options = Common.Instance.Options;
        }


        private string GetHeader(MethodEntity m) => $"Method: {m.Name}";

        private string GetOffset(Instruction i) {
            var f = (Common.Instance.Options.DecimalOffsets) ? "IL_{0:D4}" : "IL_{0:X4}";
            return String.Format(f, i.Offset);
        }

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
            return (this._options.UpcaseOpCodes) ? name.ToUpper() : name;
        }

        private string InstructionToString(Instruction i, int longestOpcode) {
            var opcodePart = this._options.AlignListing
                ? GetOpCode(i).PadRight(longestOpcode + 1)
                : GetOpCode(i);
            return $"{GetOffset(i)} {opcodePart} {GetOperand(i)}";
        }

        public void ClearSourceCache() {
            this._pdbCache.Clear();
        }

        public string Generate(MethodEntity method) {
            if (this._options.AlignListing) {
                this._longestOpCode = method.Instructions
                    .Select(i => i.OpCode.Name)
                    .Max(s => s.Length);
            }
            var sb = new StringBuilder();

            if (this._options.DisplayMethodNames) {
                sb.AppendLine(this.GetHeader(method));
                sb.AppendLine();
            }
            List<string> symbols = new List<string>();
            foreach (var instruction in method.Instructions) {
                if (this._options.IgnoreNops && instruction.OpCode.Code == Code.Nop) {
                    continue;
                }
                if (instruction.SequencePoint != null) {
                    // If hidden line
                    if (instruction.SequencePoint.StartLine == 0xfeefee) {
                        continue;
                    }
                    // If invalid Url
                    if (instruction.SequencePoint.Document?.Url == null) {
                        continue;
                    }
                    var docUrl = instruction.SequencePoint.Document.Url;
                    if (!_pdbCache.ContainsKey(docUrl)) {
                        // if file not exists
                        if (!File.Exists(docUrl)) {
                            continue;
                        }
                        _pdbCache[docUrl] = File.ReadAllLines(docUrl).Select(s => s.Trim()).ToList();
                    }
                    for (int i = instruction.SequencePoint.StartLine; i <= instruction.SequencePoint.EndLine; i++) {
                        if (symbols.Count > 0) {
                            symbols.ForEach(s => sb.AppendLine(s));
                            symbols.Clear();
                        }
                        // ignore empty lines
                        if (i < _pdbCache[docUrl].Count && !String.IsNullOrWhiteSpace(_pdbCache[docUrl][i])) {
                            symbols.Add($"// {_pdbCache[docUrl][i]}");
                        }
                    }
                }
                sb.AppendLine(InstructionToString(instruction, _longestOpCode));
            }
            return sb.ToString();
        }
    }
}
