using System;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace Msiler.Lib
{
    public static class VsThemeDetector
    {
        private static readonly Color AccentMediumDarkTheme = Color.FromRgb(45, 45, 48);
        private static readonly Color AccentMediumLightTheme = Color.FromRgb(238, 238, 242);
        private static readonly Color AccentMediumBlueTheme = Color.FromRgb(255, 236, 181);

        public static VsThemeCode GetTheme()
        {
            try
            {
                var color = VSColorTheme.GetThemedColor(EnvironmentColors.AccentMediumColorKey);
                var cc = ToColor(color);
                if (cc == AccentMediumBlueTheme)
                    return VsThemeCode.Blue;
                if (cc == AccentMediumLightTheme)
                    return VsThemeCode.Light;
                if (cc == AccentMediumDarkTheme)
                    return VsThemeCode.Dark;
                float brightness = color.GetBrightness();
                bool dark = brightness < 0.5f;
                return dark ? VsThemeCode.Dark : VsThemeCode.Light;
            }

            catch (ArgumentNullException)
            {
                return VsThemeCode.Unknown;
            }
        }

        private static Color ToColor(System.Drawing.Color color) 
            => Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
