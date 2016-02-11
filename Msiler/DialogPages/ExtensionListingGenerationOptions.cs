using Msiler.AssemblyParser;
using System.ComponentModel;

namespace Msiler.DialogPages
{
    public class ExtensionListingGenerationOptions : MsilerDialogPage
    {
        [Category("Listing generation")]
        [DisplayName("Ignore NOPs")]
        [Description("If true, Nop instrutions will be excluded from listing")]
        public bool IgnoreNops { get; set; } = false;

        [Category("Listing generation")]
        [DisplayName("Display numbers as HEX values")]
        [Description("")]
        public bool NumbersAsHex { get; set; } = false;

        [Category("Listing generation")]
        [DisplayName("Simplify function names")]
        [Description("")]
        public bool SimplifyFunctionNames { get; set; } = false;

        [Category("Listing generation")]
        [DisplayName("Upcase OpCodes")]
        [Description("If true, OpCodes will be upcased (for example LDSTR instead of ldstr)")]
        public bool UpcaseOpCodes { get; set; } = false;

        [Category("Listing generation")]
        [DisplayName("Align listing")]
        [Description("")]
        public bool AlignListing { get; set; } = true;

        [Category("Listing generation")]
        [DisplayName("Display offsets as decimal numbers")]
        [Description("")]
        public bool DecimalOffsets { get; set; } = false;

        [Category("Listing generation")]
        [DisplayName("Display method name before listing")]
        [Description("")]
        public bool IncludeMethodName { get; set; } = true;

        [Category("Listing generation")]
        [DisplayName("Read instruction bytes")]
        [Description("Can decrease performance")]
        public bool ReadInstructionBytes { get; set; } = true;

        [Category("Listing generation")]
        [DisplayName("Process PDB files")]
        [Description("NOTE: rebuilding required")]
        public bool ProcessPDBFiles { get; set; } = false;

        public ListingGeneratorOptions ToListingGeneratorOptions() {
            return new ListingGeneratorOptions {
                AlignListing = this.AlignListing,
                DecimalOffsets = this.DecimalOffsets,
                IncludeMethodName = this.IncludeMethodName,
                IgnoreNops = this.IgnoreNops,
                NumbersAsHex = this.NumbersAsHex,
                ProcessPDBFiles = this.ProcessPDBFiles,
                SimplifyFunctionNames = this.SimplifyFunctionNames,
                UpcaseOpCodes = this.UpcaseOpCodes,
                ReadInstructionBytes = this.ReadInstructionBytes
            };
        }
    }
}
