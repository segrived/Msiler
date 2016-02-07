using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.IO;
using dnlib.PE;
using System.Text;

namespace Msiler.AssemblyParser
{
    sealed class MethodBytesReader
    {
        readonly IImageStream stream;

        public MethodBytesReader(MethodDef method) {
            uint rva = (uint)method.RVA + method.Body.HeaderSize;
            this.stream = this.GetImageStream(method.Module, rva);
        }

        private IImageStream GetImageStream(ModuleDef module, uint rva) {
            var m = module as ModuleDefMD;
            if (m == null)
                return null;
            return m.MetaData.PEImage.CreateStream((RVA)rva);
        }

        public string ReadInstrution(Instruction instr) {
            if (stream == null) {
                return string.Empty;
            }
            int instructionSize = instr.GetSize();
            stream.Position = instr.Offset;
            var sb = new StringBuilder();
            for (int i = 0; i < instructionSize; i++) {
                sb.AppendFormat("{0:X2}", stream.ReadByte());
            }
            return sb.ToString();
        }

        public void Dispose() {
            if (stream != null) {
                stream.Dispose();
            }
        }
    }
}
