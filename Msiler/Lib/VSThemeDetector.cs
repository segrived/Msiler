using Microsoft.Win32;
using Msiler.Helpers;
using System.Collections.Generic;

namespace Msiler.Lib
{
    public enum VsTheme { Blue, Light, Dark, Unknown }

    public class VsThemeDetector
    {
        static readonly IDictionary<string, VsTheme> Themes = new Dictionary<string, VsTheme>{
            { "de3dbbcd-f642-433c-8353-8f1df4370aba", VsTheme.Light },
            { "1ded0138-47ce-435e-84ef-9ec1f439b749", VsTheme.Dark },
            { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VsTheme.Blue }
        };

        static VsTheme GuidToThemeName(string guid) {
            return !Themes.ContainsKey(guid) ? VsTheme.Unknown : Themes[guid];
        }

        VsTheme VisualStudio2012Theme() {
            const string rKey = @"Software\Microsoft\VisualStudio\11.0\General";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                string keyText = (string)key?.GetValue("CurrentTheme", string.Empty);
                if (!string.IsNullOrEmpty(keyText)) {
                    return GuidToThemeName(keyText);
                }
            }
            return VsTheme.Unknown;
        }

        VsTheme VisualStudio2013Theme() {
            const string rKey = @"Software\Microsoft\VisualStudio\12.0\General";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                string keyText = (string)key?.GetValue("CurrentTheme", string.Empty);
                if (!string.IsNullOrEmpty(keyText)) {
                    return GuidToThemeName(keyText.Replace("{", "").Replace("}", ""));
                }
            }
            return VsTheme.Unknown;
        }

        VsTheme VisualStudio2015Theme() {
            const string rKey = @"Software\Microsoft\VisualStudio\14.0\ApplicationPrivateSettings\Microsoft\VisualStudio";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                string keyText = (string)key?.GetValue("ColorTheme", string.Empty);

                if (string.IsNullOrEmpty(keyText)) {
                    return VsTheme.Unknown;
                }
                var keyTextValues = keyText.Split('*');
                if (keyTextValues.Length > 2) {
                    return GuidToThemeName(keyTextValues[2]);
                }
            }
            return VsTheme.Unknown;
        }

        public static VsTheme GetTheme() {
            string version = DteHelpers.GetDte().Application.Version;
            switch (version) {
                case "14.0":
                    return new VsThemeDetector().VisualStudio2015Theme();
                case "12.0":
                    return new VsThemeDetector().VisualStudio2013Theme();
                case "11.0":
                    return new VsThemeDetector().VisualStudio2012Theme();
                default:
                    return VsTheme.Unknown;
            }
        }
    }
}
