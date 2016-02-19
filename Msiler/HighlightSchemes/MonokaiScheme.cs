using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msiler.HighlightSchemes
{
    class MonokaiScheme : IListingHighlightingScheme
    {
        internal class Scheme : IListingHighlightingSchemeDef
        {
            public string BuiltinTypeHighlight { get; } = "#AE81FF;BI";
            public string CommentHighlight { get; } = "#75715E;I";
            public string NumericHighlight { get; } = "#FD971F;B";
            public string OffsetHighlight { get; } = "#F92672;B";
            public string OpCodeHighlight { get; } = "#66D9EF;B";
            public string StringHighlight { get; } = "#A6E22E";
            public string ErrorHighlight { get; } = "#F92672";

        }


        public IListingHighlightingSchemeDef GetScheme() {
            return new Scheme();
        }
    }
}
