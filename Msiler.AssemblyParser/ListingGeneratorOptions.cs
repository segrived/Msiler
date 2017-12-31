namespace Msiler.AssemblyParser
{
    public class ListingGeneratorOptions
    {
        public bool IgnoreNops { get; set; } = false;
        public bool NumbersAsHex { get; set; } = false;
        public bool SimplifyFunctionNames { get; set; } = false;
        public bool UpcaseOpCodes { get; set; } = false;
        public bool AlignListing { get; set; } = true;
        public bool DecimalOffsets { get; set; } = false;
        public bool IncludeMethodName { get; set; } = false;
        public bool ProcessPdbFiles { get; set; } = false;
        public bool ReadInstructionBytes { get; set; } = false;
        public DescriptionMode DescriptionMode { get; set; } = DescriptionMode.Disabled;
    }

    public enum DescriptionMode
    {
        Disabled,
        BeforeLine,
        AfterInstruction
    }
}