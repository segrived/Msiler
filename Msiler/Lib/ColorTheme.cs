using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;

namespace Quart.Msiler.Lib
{
    public static class ColorTheme
    {
        public static Dictionary<string, Color> DarkTheme = new Dictionary<string, Color> {
            ["Comment"] = Color.FromRgb(93, 130, 221),
            ["String"] = Color.FromRgb(177, 233, 98),
            ["Offset"] = Color.FromRgb(242, 248, 104),
            ["Instruction"] = Color.FromRgb(93, 181, 222),
            ["Number"] = Color.FromRgb(232, 97, 153),
            ["BuiltInTypes"] = Color.FromRgb(150, 150, 150),
            ["Header"] = Color.FromRgb(110, 110, 110)
        };

        public static Dictionary<string, Color> LightTheme = new Dictionary<string, Color> {
            ["Comment"] = Color.FromRgb(114, 128, 158),
            ["String"] = Color.FromRgb(35, 145, 63),
            ["Offset"] = Color.FromRgb(128, 35, 145),
            ["Instruction"] = Color.FromRgb(171, 124, 41),
            ["Number"] = Color.FromRgb(163, 168, 41),
            ["BuiltInTypes"] = Color.FromRgb(90, 90, 90),
            ["Header"] = Color.FromRgb(130, 130, 130)
        };

        public static IHighlightingDefinition GetColorTheme(MsilerColorTheme mt) {
            VisualStudioTheme theme;
            var high = GetDefaultHighlightingDefinition();
            if (mt != MsilerColorTheme.Auto) {
                theme = mt == MsilerColorTheme.Dark ? VisualStudioTheme.Dark : VisualStudioTheme.Light;
            } else {

                theme = VSThemeDetector.GetTheme();
                if (theme == VisualStudioTheme.Unknown) {
                    theme = VisualStudioTheme.Light; // if unknown
                }
            }
            if (theme == VisualStudioTheme.Light || theme == VisualStudioTheme.Blue) {
                ApplyColorTheme(high, LightTheme);
            } else {
                ApplyColorTheme(high, DarkTheme);
            }
            return high;
        }

        private static void ApplyColorTheme(IHighlightingDefinition def, Dictionary<string, Color> theme) {
            foreach (var kv in theme) {
                def.GetNamedColor(kv.Key).Foreground = new SimpleHighlightingBrush(kv.Value);
            }
        }

        public static IHighlightingDefinition GetDefaultHighlightingDefinition() {
            var ilRes = "Quart.Msiler.Resources.IL.xshd";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ilRes)) {
                using (var reader = new System.Xml.XmlTextReader(stream)) {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }
}
