namespace Msiler.AssemblyParser.SimpleListingGenerator
{
    public class GeneratorOptions
    {
        public bool IgnoreNops { get; set; } = false;
        public bool NumbersAsHex { get; set; } = false;
        public bool SimplifyFunctionNames { get; set; } = false;
        public bool UpcaseOpCodes { get; set; } = false;
        public bool AlignListing { get; set; } = true;
        public bool DecimalOffsets { get; set; } = false;
        public bool DisplayMethodNames { get; set; } = false;
        public bool ProcessPDBFiles { get; set; } = false;
    }
}
