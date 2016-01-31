using dnlib.DotNet;
using dnlib.IO;

namespace Quart.Msiler.Lib
{
    sealed class InstructionBytesReader
    {

        readonly IImageStream stream;

        public InstructionBytesReader(MethodDef method) {
            this.stream = method.Module.GetImageStream((uint)method.RVA + method.Body.HeaderSize);
        }

        public int ReadByte() {
            if (stream != null)
                return stream.ReadByte();
            return -1;
        }

        public void SetInstruction(int index, uint offset) {
            if (stream != null)
                stream.Position = offset;
        }

        public void Dispose() {
            if (stream != null)
                stream.Dispose();
        }
    }
}
