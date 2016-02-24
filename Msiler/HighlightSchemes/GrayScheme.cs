namespace Msiler.HighlightSchemes
{
    class GrayScheme : IListingHighlightingScheme
    {
        internal class Scheme : IListingHighlightingSchemeDef
        {
            public string BuiltinTypeHighlight { get; } = "#A7A7A7;BI";
            public string CommentHighlight { get; } = "#BBBBBB;I";
            public string NumericHighlight { get; } = "#AAAAAA;B";
            public string OffsetHighlight { get; } = "#555555;B";
            public string OpCodeHighlight { get; } = "#888888;B";
            public string StringHighlight { get; } = "#666666";
            public string ErrorHighlight { get; } = "#777777;B";
            public string WarningHighlight { get; } = "#777777;B";
        }


        public IListingHighlightingSchemeDef GetScheme() {
            return new Scheme();
        }
    }
}
