using Msiler.Helpers;

namespace Msiler.HighlightSchemes
{
    internal class DefaultAutoScheme : IListingHighlightingScheme
    {
        public IListingHighlightingSchemeDef GetScheme()
        {
            var theme = VsThemeHelpers.GetTheme();

            if (theme == VsThemeCode.Light || theme == VsThemeCode.Blue || theme == VsThemeCode.Unknown)
                return new DefaultLightScheme().GetScheme();

            return new DefaultDarkScheme().GetScheme();
        }
    }
}