using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.IO;
using dnlib.PE;
using System.Text;

namespace Msiler.AssemblyParser
{
    internal sealed class MethodBytesReader
    {
        private readonly IImageStream stream;

        public MethodBytesReader(MethodDef method)
        {
            uint rva = (uint)method.RVA + method.Body.HeaderSize;
            this.stream = GetImageStream(method.Module, rva);
        }

        private static IImageStream GetImageStream(ModuleDef module, uint rva)
        {
            if (!(module is ModuleDefMD m) || m.MetaData == null)
                return null;

            return m.MetaData.PEImage.CreateStream((RVA)rva);
        }

        public string ReadInstrution(Instruction instr)
        {
            if (this.stream == null)
                return string.Empty;

            int instructionSize = instr.GetSize();
            this.stream.Position = instr.Offset;
            var sb = new StringBuilder();

            for (int i = 0; i < instructionSize; i++)
                sb.AppendFormat("{0:X2}", this.stream.ReadByte());

            return sb.ToString();
        }

        public void Dispose() => this.stream?.Dispose();
    }
}
