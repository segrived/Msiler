using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb;
using System.IO;

namespace Msiler.AssemblyParser
{
    public class ListingGenerator : IDisposable
    {
        private readonly ListingGeneratorOptions _options;
        private MethodBytesReader _bytesReader;

        private readonly Dictionary<string, List<string>> _pdbCache =
            new Dictionary<string, List<string>>();

        private int _longestOpCode = -1;
        private int _longestByteSeq = -1;

        public ListingGenerator(ListingGeneratorOptions options) {
            this._options = options;
        }

        private string GetHeader(AssemblyMethod m) => $"Method: {m.Signature.MethodName}";

        private string GetOffset(Instruction i) {
            var f = (this._options.DecimalOffsets) ? "IL_{0:D4}" : "IL_{0:X4}";
            return String.Format(f, i.Offset);
        }

        private string GetOperand(Instruction i) {
            if (i.OpCode.Code == Code.Ldstr) {
                return @"""" + i.Operand.ToString().ReplaceNewLineCharacters() + @"""";
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

            if (i.Operand is MethodDef) {
                var m = (MethodDef)i.Operand;
                var assemblyMethod = new AssemblyMethod(m);
                if (this._options.SimplifyFunctionNames) {
                    return assemblyMethod.Signature.MethodName;
                }
                return assemblyMethod.Signature.ToString();
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
            var result = $"{GetOffset(i)} ";
            if (this._options.ReadInstructionBytes && this._bytesReader != null) {
                var bytesSeq = $"{this._bytesReader.ReadInstrution(i)} ";
                if (this._options.AlignListing) {
                    bytesSeq = bytesSeq.PadRight(_longestByteSeq + 1);
                }
                result += $"| {bytesSeq}| ";
            }
            var opcodePart = this._options.AlignListing
                ? GetOpCode(i).PadRight(longestOpcode + 1)
                : GetOpCode(i);
            result += $"{opcodePart} ";
            return result + $"{GetOperand(i)}";
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

        public string GenerateListing(AssemblyMethod method) {
            var sb = new StringBuilder();

            if (this._options.DisplayMethodNames) {
                sb.AppendLine($"// Selected method: {method.Signature.MethodName}");
                sb.AppendLine();
            }

            if (this._options.ReadInstructionBytes) {
                this._bytesReader = new MethodBytesReader(method.MethodDefinition);
            }

            if (this._options.AlignListing) {
                this._longestOpCode = method.Instructions.Max(i => i.OpCode.Name.Length);
                if (this._options.ReadInstructionBytes) {
                    this._longestByteSeq = method.Instructions.Max(i => i.GetSize()) * 2;
                }
            }

            foreach (var instruction in method.Instructions) {
                if (this._options.IgnoreNops && instruction.OpCode.Code == Code.Nop) {
                    continue;
                }
                if (this._options.ProcessPDBFiles) {
                    sb.Append(this.ParsePdbInformation(instruction.SequencePoint));
                }
                sb.AppendLine(InstructionToString(instruction, _longestOpCode));
            }

            return sb.ToString();
        }

        public void Dispose() {
            if (this._bytesReader != null) {
                this._bytesReader.Dispose();
            }
        }
    }
}
