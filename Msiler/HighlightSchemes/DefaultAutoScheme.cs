using Msiler.Lib;

namespace Msiler.HighlightSchemes
{
    class DefaultAutoScheme : IListingHighlightingScheme
    {
        public IListingHighlightingSchemeDef GetScheme() {
            var theme = VsThemeDetector.GetTheme();
            if (theme == VsThemeCode.Light || theme == VsThemeCode.Blue || theme == VsThemeCode.Unknown) {
                return new DefaultLightScheme().GetScheme();
            }
            return new DefaultDarkScheme().GetScheme();
        }
    }
}