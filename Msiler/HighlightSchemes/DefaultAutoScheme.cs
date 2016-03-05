using Msiler.Lib;

namespace Msiler.HighlightSchemes
{
    class DefaultAutoScheme : IListingHighlightingScheme
    {
        public IListingHighlightingSchemeDef GetScheme() {
            var theme = VsThemeDetector.GetTheme();
            if (theme == VsTheme.Light || theme == VsTheme.Blue || theme == VsTheme.Unknown) {
                return new DefaultLightScheme().GetScheme();
            }
            return new DefaultDarkScheme().GetScheme();
        }
    }
}