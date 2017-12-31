using Msiler.AssemblyParser;
using System.ComponentModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Msiler.DialogPages
{
    public class ExtensionListingGenerationOptions : MsilerDialogPage
    {
        private const string CATEGORY_TITLE = "Listing Generation";

        [Category(CATEGORY_TITLE)]
        [DisplayName("Ignore NOPs")]
        [Description("If true, Nop instrutions will be excluded from listing")]
        public bool IgnoreNops { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Display numbers as HEX values")]
        [Description("")]
        public bool NumbersAsHex { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Simplify function names")]
        [Description("")]
        public bool SimplifyFunctionNames { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Upcase OpCodes")]
        [Description("If true, OpCodes will be upcased (for example LDSTR instead of ldstr)")]
        public bool UpcaseOpCodes { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Align listing")]
        [Description("")]
        public bool AlignListing { get; set; } = true;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Display offsets as decimal numbers")]
        [Description("")]
        public bool DecimalOffsets { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Display method name before listing")]
        [Description("")]
        public bool IncludeMethodName { get; set; } = true;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Read instruction bytes")]
        [Description("Can decrease performance")]
        public bool ReadInstructionBytes { get; set; } = false;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Process PDB files")]
        [Description("NOTE: rebuilding required")]
        public bool ProcessPdbFiles { get; set; } = true;

        [Category(CATEGORY_TITLE)]
        [DisplayName("Instruction description mode")]
        [Description("If enabled, instruction description will be included directly in listing")]
        public DescriptionMode InstructionDescriptionMode { get; set; } = DescriptionMode.Disabled;

        public ListingGeneratorOptions ToListingGeneratorOptions() 
            => new ListingGeneratorOptions
        {
            AlignListing = this.AlignListing,
            DecimalOffsets = this.DecimalOffsets,
            IncludeMethodName = this.IncludeMethodName,
            IgnoreNops = this.IgnoreNops,
            NumbersAsHex = this.NumbersAsHex,
            ProcessPdbFiles = this.ProcessPdbFiles,
            SimplifyFunctionNames = this.SimplifyFunctionNames,
            UpcaseOpCodes = this.UpcaseOpCodes,
            ReadInstructionBytes = this.ReadInstructionBytes,
            DescriptionMode = this.InstructionDescriptionMode
        };
    }
}
