using System;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace Msiler.Helpers
{
    public static class VsThemeHelpers
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

                return color.GetBrightness() < 0.5f ? VsThemeCode.Dark : VsThemeCode.Light;
            }
            catch (Exception ex)
            {
                return VsThemeCode.Unknown;
            }
        }

        public static Color GetHightlightColor(System.Drawing.Color color, int transLevel)
        {
            bool isDark = color.GetBrightness() < 0.5f;
            color = isDark ? ControlPaint.Light(color, 0.4f) : ControlPaint.Dark(color, 0.2f);
            return System.Drawing.Color.FromArgb(transLevel, color.R, color.G, color.B).ToColor();
        }

        public static Color ToColor(this System.Drawing.Color color) 
            => Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public enum VsThemeCode { Blue, Light, Dark, Unknown }
}
