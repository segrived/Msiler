namespace Msiler.HighlightSchemes
{
    public class DefaultLightScheme : IListingHighlightingScheme
    {
        public IListingHighlightingSchemeDef GetScheme() => new Scheme();

        internal class Scheme : IListingHighlightingSchemeDef
        {
            public string BuiltinTypeHighlight { get; } = "#5A5A5A;B";
            public string CommentHighlight { get; } = "#72809E;I";
            public string NumericHighlight { get; } = "#A3A829;B";
            public string OffsetHighlight { get; } = "#802391;B";
            public string OpCodeHighlight { get; } = "#AB7C29;B";
            public string StringHighlight { get; } = "#23913F";
        }
    }
}
