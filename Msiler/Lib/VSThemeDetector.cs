using Microsoft.Win32;
using Msiler.Helpers;
using System.Collections.Generic;

namespace Msiler.Lib
{
    public enum VSTheme { Blue, Light, Dark, Unknown }

    public class VSThemeDetector
    {
        static readonly IDictionary<string, VSTheme> Themes = new Dictionary<string, VSTheme>{
            { "de3dbbcd-f642-433c-8353-8f1df4370aba", VSTheme.Light },
            { "1ded0138-47ce-435e-84ef-9ec1f439b749", VSTheme.Dark },
            { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VSTheme.Blue }
        };

        VSTheme GuidToThemeName(string guid) {
            if (!Themes.ContainsKey(guid)) {
                return VSTheme.Unknown;
            }
            return Themes[guid];
        }

        VSTheme VisualStudio2012Theme() {
            var rKey = @"Software\Microsoft\VisualStudio\11.0\General";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                if (key != null) {
                    var keyText = (string)key.GetValue("CurrentTheme", string.Empty);
                    if (!string.IsNullOrEmpty(keyText)) {
                        return this.GuidToThemeName(keyText);
                    }
                }
            }
            return VSTheme.Unknown;
        }

        VSTheme VisualStudio2013Theme() {
            var rKey = @"Software\Microsoft\VisualStudio\12.0\General";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                if (key != null) {
                    var keyText = (string)key.GetValue("CurrentTheme", string.Empty);
                    if (!string.IsNullOrEmpty(keyText)) {
                        return this.GuidToThemeName(keyText.Replace("{", "").Replace("}", ""));
                    }
                }
            }
            return VSTheme.Unknown;
        }

        VSTheme VisualStudio2015Theme() {
            var rKey = @"Software\Microsoft\VisualStudio\14.0\ApplicationPrivateSettings\Microsoft\VisualStudio";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                if (key != null) {
                    var keyText = (string)key.GetValue("ColorTheme", string.Empty);

                    if (!string.IsNullOrEmpty(keyText)) {
                        var keyTextValues = keyText.Split('*');
                        if (keyTextValues.Length > 2) {
                            return this.GuidToThemeName(keyTextValues[2]);
                        }
                    }
                }
            }
            return VSTheme.Unknown;
        }

        public static VSTheme GetTheme() {
            var version = DTEHelpers.GetDTE().Application.Version;
            switch (version) {
                case "14.0":
                    return new VSThemeDetector().VisualStudio2015Theme();
                case "12.0":
                    return new VSThemeDetector().VisualStudio2013Theme();
                case "11.0":
                    return new VSThemeDetector().VisualStudio2012Theme();
                default:
                    return VSTheme.Unknown;
            }
        }
    }
}
