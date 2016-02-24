namespace Msiler.HighlightSchemes
{
    public class DefaultDarkScheme : IListingHighlightingScheme
    {
        public IListingHighlightingSchemeDef GetScheme() => new Scheme();

        internal class Scheme : IListingHighlightingSchemeDef
        {
            public string BuiltinTypeHighlight { get; } = "#969696;B";
            public string CommentHighlight { get; } = "#5D82DD;I";
            public string NumericHighlight { get; } = "#E86199;B";
            public string OffsetHighlight { get; } = "#F2F868;B";
            public string OpCodeHighlight { get; } = "#5DB5DE;B";
            public string StringHighlight { get; } = "#B1E662";
            public string ErrorHighlight { get; } = "#FF6666;B";
            public string WarningHighlight { get; } = "#FFC266;B";
        }
    }
}
