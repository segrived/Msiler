using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using dnlib.DotNet;
using dnlib.DotNet.Pdb;

namespace Quart.Msiler.Lib
{
    public class ListingGenerator
    {
        private int _longestOpCode;

        private readonly MsilerOptions _options;

        private readonly Dictionary<string, List<string>> _pdbCache =
            new Dictionary<string, List<string>>();

        public ListingGenerator() {
            this._options = Common.Instance.Options;
        }


        private string GetHeader(MethodEntity m) => $"Method: {m.Signature.Name}";

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

            if (this._options.SimplifyFunctionNames && (i.Operand is MethodDef)) {
                var m = (MethodDef)i.Operand;
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

        private string ParsePdbInformation(SequencePoint sp) {
            if (sp == null) {
                return String.Empty;
            }
            // If hidden line
            if (sp.StartLine == 0xfeefee) {
                return String.Empty;
            }
            // If invalid Url
            if (sp.Document?.Url == null) {
                return String.Empty;
            }
            var docUrl = sp.Document.Url;
            if (!_pdbCache.ContainsKey(sp.Document.Url)) {
                // if file not exists
                if (!File.Exists(sp.Document.Url)) {
                    return String.Empty;
                }
                _pdbCache[docUrl] = File.ReadAllLines(docUrl).Select(s => s.Trim()).ToList();
            }
            var sb = new StringBuilder();
            for (int i = sp.StartLine - 1; i <= sp.EndLine - 1; i++) {
                if (i < _pdbCache[docUrl].Count && !String.IsNullOrWhiteSpace(_pdbCache[docUrl][i])) {
                    sb.AppendLine($"// {_pdbCache[docUrl][i]}");
                }
            }
            return sb.ToString();
        }

        public string Generate(MethodEntity method) {
            if (this._options.AlignListing) {
                this._longestOpCode = method.Instructions
                    .Select(i => i.OpCode.Name)
                    .Max(s => s.Length);
            }
            var sb = new StringBuilder();

            foreach (var instruction in method.Instructions) {
                if (this._options.IgnoreNops && instruction.OpCode.Code == Code.Nop) {
                    continue;
                }
                sb.Append(this.ParsePdbInformation(instruction.SequencePoint));
                sb.AppendLine(InstructionToString(instruction, _longestOpCode));
            }
            return sb.ToString();
        }
    }
}
