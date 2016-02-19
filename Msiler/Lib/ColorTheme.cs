using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Msiler.HighlightSchemes;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Msiler.Lib
{
    public enum MsilerColorTheme
    {
        Auto, DefaultLight, DefaultDark, Monokai, Gray
    }

    public static class ColorTheme
    {
        private static readonly Regex highlightingColorRegex =
            new Regex(@"(?<Color>#[\da-f]{6})($|;(?<Flags>[BUI]*))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static IListingHighlightingScheme GetHighlightingScheme(MsilerColorTheme theme) {
            switch (theme) {
                case MsilerColorTheme.Auto:
                    return new DefaultAutoScheme();
                case MsilerColorTheme.DefaultDark:
                    return new DefaultDarkScheme();
                case MsilerColorTheme.DefaultLight:
                    return new DefaultLightScheme();
                case MsilerColorTheme.Monokai:
                    return new MonokaiScheme();
                case MsilerColorTheme.Gray:
                    return new GrayScheme();
            }
            return new DefaultAutoScheme(); // will not executed
        }

        public static IHighlightingDefinition GetDefinition(IListingHighlightingScheme scheme) {
            var highDef = GetDefaultHighlightingDefinition();
            var schemeDef = scheme.GetScheme();
            highDef.ApplySchemeEntry("Comment", schemeDef.CommentHighlight);
            highDef.ApplySchemeEntry("String", schemeDef.StringHighlight);
            highDef.ApplySchemeEntry("Offset", schemeDef.OffsetHighlight);
            highDef.ApplySchemeEntry("OpCode", schemeDef.OpCodeHighlight);
            highDef.ApplySchemeEntry("Numeric", schemeDef.NumericHighlight);
            highDef.ApplySchemeEntry("BuiltInType", schemeDef.BuiltinTypeHighlight);
            highDef.ApplySchemeEntry("Error", schemeDef.ErrorHighlight);
            return highDef;
        }

        private static void ApplySchemeEntry(this IHighlightingDefinition def, string rule, string entry) {
            def.GetNamedColor(rule).MergeWith(StringToHighlightingColor(entry));
        }

        private static HighlightingColor StringToHighlightingColor(string s) {
            var match = highlightingColorRegex.Match(s);
            if (!match.Success) {
                return null;
            }
            var colorStr = match.Groups["Color"].Value;
            var flagsStr = match.Groups["Flags"].Value;
            var color = (Color)ColorConverter.ConvertFromString(colorStr);

            bool isBold = flagsStr.Contains("B");
            bool isItalic = flagsStr.Contains("I");
            bool isUnderline = flagsStr.Contains("U");

            return new HighlightingColor {
                Foreground = new SimpleHighlightingBrush(color),
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                Underline = isUnderline
            };
        }

        private static IHighlightingDefinition GetDefaultHighlightingDefinition() {
            var ilRes = "Msiler.Resources.IL.xshd";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ilRes)) {
                using (var reader = new System.Xml.XmlTextReader(stream)) {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }
}
