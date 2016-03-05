using Msiler.AssemblyParser;
using System.ComponentModel;

namespace Msiler.DialogPages
{
    public class ExtensionListingGenerationOptions : MsilerDialogPage
    {
        private const string CategoryTitle = "Listing Generation";

        [Category(CategoryTitle)]
        [DisplayName("Ignore NOPs")]
        [Description("If true, Nop instrutions will be excluded from listing")]
        public bool IgnoreNops { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Display numbers as HEX values")]
        [Description("")]
        public bool NumbersAsHex { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Simplify function names")]
        [Description("")]
        public bool SimplifyFunctionNames { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Upcase OpCodes")]
        [Description("If true, OpCodes will be upcased (for example LDSTR instead of ldstr)")]
        public bool UpcaseOpCodes { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Align listing")]
        [Description("")]
        public bool AlignListing { get; set; } = true;

        [Category(CategoryTitle)]
        [DisplayName("Display offsets as decimal numbers")]
        [Description("")]
        public bool DecimalOffsets { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Display method name before listing")]
        [Description("")]
        public bool IncludeMethodName { get; set; } = true;

        [Category(CategoryTitle)]
        [DisplayName("Read instruction bytes")]
        [Description("Can decrease performance")]
        public bool ReadInstructionBytes { get; set; } = false;

        [Category(CategoryTitle)]
        [DisplayName("Process PDB files")]
        [Description("NOTE: rebuilding required")]
        public bool ProcessPdbFiles { get; set; } = true;

        public ListingGeneratorOptions ToListingGeneratorOptions() {
            return new ListingGeneratorOptions {
                AlignListing = this.AlignListing,
                DecimalOffsets = this.DecimalOffsets,
                IncludeMethodName = this.IncludeMethodName,
                IgnoreNops = this.IgnoreNops,
                NumbersAsHex = this.NumbersAsHex,
                ProcessPDBFiles = this.ProcessPdbFiles,
                SimplifyFunctionNames = this.SimplifyFunctionNames,
                UpcaseOpCodes = this.UpcaseOpCodes,
                ReadInstructionBytes = this.ReadInstructionBytes
            };
        }
    }
}
