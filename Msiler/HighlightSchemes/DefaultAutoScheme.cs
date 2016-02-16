using Msiler.Lib;

namespace Msiler.HighlightSchemes
{
    class DefaultAutoScheme : IListingHighlightingScheme
    {
        public IListingHighlightingSchemeDef GetScheme() {
            var theme = VSThemeDetector.GetTheme();
            if (theme == VSTheme.Light || theme == VSTheme.Blue || theme == VSTheme.Unknown) {
                return new DefaultLightScheme().GetScheme();
            }
            return new DefaultDarkScheme().GetScheme();
        }
    }
}