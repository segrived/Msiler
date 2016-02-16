using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Msiler.HighlightSchemes;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Msiler.Lib
{
    public static class ColorTheme
    {
        private static readonly Regex highlightingColorRegex =
            new Regex(@"(?<Color>#[\da-f]{6})($|;(?<Flags>[BUI]*))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void GetColorTheme(IListingHighlightingScheme scheme) {
            var defaultTheme = GetDefaultHighlightingDefinition();
            var schemeDef = scheme.GetScheme();

            defaultTheme.GetNamedColor("Comment").MergeWith(StringToHighlightingColor(schemeDef.CommentHighlight));
            defaultTheme.GetNamedColor("String").MergeWith(StringToHighlightingColor(schemeDef.StringHighlight));
            defaultTheme.GetNamedColor("Offset").MergeWith(StringToHighlightingColor(schemeDef.OffsetHighlight));
            defaultTheme.GetNamedColor("Instruction").MergeWith(StringToHighlightingColor(schemeDef.OpCodeHighlight));
            defaultTheme.GetNamedColor("Number").MergeWith(StringToHighlightingColor(schemeDef.NumericHighlight));
            defaultTheme.GetNamedColor("BuiltInTypes").MergeWith(StringToHighlightingColor(schemeDef.BuiltinTypeHighlight));
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
