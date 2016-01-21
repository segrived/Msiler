using Microsoft.Win32;
using System.Collections.Generic;

namespace Quart.Msiler.Lib
{
    public enum VisualStudioTheme { Blue, Light, Dark, Unknown }

    public class VSThemeDetector
    {
        private static readonly IDictionary<string, VisualStudioTheme> Themes = new Dictionary<string, VisualStudioTheme>{
            { "de3dbbcd-f642-433c-8353-8f1df4370aba", VisualStudioTheme.Light },
            { "1ded0138-47ce-435e-84ef-9ec1f439b749", VisualStudioTheme.Dark },
            { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VisualStudioTheme.Blue }
        };

        private VisualStudioTheme GuidToThemeName(string guid) {
            if (!Themes.ContainsKey(guid)) {
                return VisualStudioTheme.Unknown;
            }
            return Themes[guid];
        }


        private VisualStudioTheme VisualStudio2013Theme() {
            var rKey = @"Software\Microsoft\VisualStudio\12.0\General";

            using (var key = Registry.CurrentUser.OpenSubKey(rKey)) {
                if (key != null) {
                    var keyText = (string)key.GetValue("CurrentTheme", string.Empty);
                    if (!string.IsNullOrEmpty(keyText)) {
                        return this.GuidToThemeName(keyText.Replace("{", "").Replace("}", ""));
                    }
                }
            }

            return VisualStudioTheme.Unknown;
        }

        private VisualStudioTheme VisualStudio2015Theme() {
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

            return VisualStudioTheme.Unknown;
        }

        public static VisualStudioTheme GetTheme() {
            var version = Helpers.GetDTE().Application.Version;
            if (version == "14.0") {
                return new VSThemeDetector().VisualStudio2015Theme();
            } else if (version == "12.0") {
                return new VSThemeDetector().VisualStudio2013Theme();
            } else {
                return VisualStudioTheme.Unknown;
            }
        }
    }
}
