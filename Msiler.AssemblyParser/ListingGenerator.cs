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
    internal struct PdbCheckSumAlgorithms
    {
        public static Guid Md5Guid  = new Guid(0x406ea660, 0x64cf, 0x4c82, 0xb6, 0xf0, 0x42, 0xd4, 0x81, 0x72, 0xa7, 0x99);
        public static Guid Sha1Guid = new Guid(0xff1816ec, 0xaa5e, 0x4d10, 0x87, 0xf7, 0x6f, 0x49, 0x63, 0x83, 0x34, 0x60);
    }

    public class ListingGenerator : IDisposable
    {
        private readonly HashSet<string> warnings = new HashSet<string>();

        private readonly ListingGeneratorOptions options;
        private MethodBytesReader bytesReader;

        private readonly Dictionary<string, List<string>> pdbCache = new Dictionary<string, List<string>>();

        private int longestOpCode = -1;
        private int longestByteSeq = -1;

        public ListingGenerator(ListingGeneratorOptions options)
        {
            this.options = options;
        }

        private string GetHeader(AssemblyMethod m) => $"Method: {m.Signature.MethodName}";

        private string GetOffset(Instruction i) {
            string f = this.options.DecimalOffsets ? "IL_{0:D4}" : "IL_{0:X4}";
            return String.Format(f, i.Offset);
        }

        private string GetOperand(Instruction i)
        {
            if (i.OpCode.Code == Code.Ldstr)
                return @"""" + i.Operand.ToString().ReplaceNewLineCharacters() + @"""";

            switch (i.Operand)
            {
                case null:
                    return String.Empty;
                case Instruction instruction:
                    return this.GetOffset(instruction);
                case Instruction[] operands:
                    return $"[ {String.Join(" | ", operands.Select(this.GetOffset))} ]";
                case IMemberRef op:
                    string simpleName = op is ITypeDefOrRef ? op.FullName : $"{op.DeclaringType.FullName}.{op.Name}";
                    return this.options.SimplifyFunctionNames ? simpleName : op.FullName;
            }

            if (this.options.NumbersAsHex)
            {
                bool isNumeric = long.TryParse(i.Operand.ToString(), out long number);
                if (isNumeric)
                    return $"0x{number:X}";
            }

            return i.Operand.ToString();
        }

        private string GetOpCode(Instruction i) => this.options.UpcaseOpCodes ? i.OpCode.Name.ToUpper() : i.OpCode.Name;

        private string InstructionToString(Instruction i, int longestOpcode)
        {
            string result = $"{this.GetOffset(i)} ";
            if (this.options.ReadInstructionBytes && this.bytesReader != null)
            {
                string bytesSeq = $"{this.bytesReader.ReadInstrution(i)} ";
                if (this.options.AlignListing)
                    bytesSeq = bytesSeq.PadRight(this.longestByteSeq + 1);
                result += $"| {bytesSeq}| ";
            }
            string opcodePart = this.options.AlignListing ? this.GetOpCode(i).PadRight(longestOpcode + 1) : this.GetOpCode(i);
            return result + $"{opcodePart} {this.GetOperand(i)}";
        }

        private string ParsePdbInformation(SequencePoint sp)
        {
            if (sp == null)
                return String.Empty;

            // If hidden line
            if (sp.StartLine == 0xfeefee)
                return String.Empty;

            // If invalid Url
            if (sp.Document?.Url == null)
                return String.Empty;

            string docUrl = sp.Document.Url;

            if (!this.pdbCache.ContainsKey(sp.Document.Url))
            {
                // if file not exists
                if (!File.Exists(docUrl))
                    return String.Empty;

                byte[] currentDocumentHash = new byte[0];
                if (sp.Document.CheckSumAlgorithmId == PdbCheckSumAlgorithms.Md5Guid)
                    currentDocumentHash = Helpers.ComputeMd5FileHash(docUrl);
                else if (sp.Document.CheckSumAlgorithmId == PdbCheckSumAlgorithms.Sha1Guid)
                    currentDocumentHash = Helpers.ComputeSha1FileHash(docUrl);

                // display warning if source file was changed
                if (!Helpers.IsByteArraysEqual(currentDocumentHash, sp.Document.CheckSum))
                    this.warnings.Add($"WARNING: Document {Path.GetFileName(docUrl)} was changed, PDB information can be incorrect.");

                this.pdbCache[docUrl] = File.ReadAllLines(docUrl).Select(s => s.Trim()).ToList();
            }

            var sb = new StringBuilder();
            for (int i = sp.StartLine - 1; i <= sp.EndLine - 1; i++)
            {
                string sourceLine = this.pdbCache[docUrl][i];
                if (i < this.pdbCache[docUrl].Count 
                    && !String.IsNullOrWhiteSpace(sourceLine) 
                    && !sourceLine.StartsWith("//", StringComparison.Ordinal))
                {
                    sb.AppendLine($"// {sourceLine}");
                }
            }
            return sb.ToString();
        }

        public string GenerateListing(AssemblyMethod method)
        {
            var sb = new StringBuilder();

            if (this.options.IncludeMethodName)
            {
                sb.AppendLine($"// Selected method: {method.Signature.MethodName}");
                sb.AppendLine();
            }

            if (this.options.ReadInstructionBytes)
                this.bytesReader = new MethodBytesReader(method.MethodDefinition);

            if (this.options.AlignListing)
            {
                this.longestOpCode = method.Instructions.Max(i => i.OpCode.Name.Length);
                if (this.options.ReadInstructionBytes)
                    this.longestByteSeq = method.Instructions.Max(i => i.GetSize()) * 2;
            }

            foreach (var instruction in method.Instructions)
            {
                if (this.options.IgnoreNops && instruction.OpCode.Code == Code.Nop)
                    continue;

                if (this.options.ProcessPdbFiles)
                    sb.Append(this.ParsePdbInformation(instruction.SequencePoint));

                sb.AppendLine(this.InstructionToString(instruction, this.longestOpCode));
            }

            string resultListing = sb.ToString();

            if (this.warnings.Count <= 0)
                return resultListing;

            return $"{String.Join(Environment.NewLine, this.warnings)}{Environment.NewLine}{sb}";
        }

        public void Dispose() => this.bytesReader?.Dispose();
    }
}
