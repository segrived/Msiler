using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace Quart.Msiler
{
    public partial class MyControl : UserControl
    {
        private const string RepoUrl = @"https://github.com/segrived/Msiler";

        private readonly MyControlVM _viewModel;

        ToolTip toolTip = new ToolTip();
        Regex lineRegex =
            new Regex(@"^IL_(?<Offset>[a-f\d]+)\s+(?<Instruction>[a-z\d.]+)\s+(?<Operand>.*)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Dictionary<string, Color> DarkTheme = new Dictionary<string, Color> {
            ["Comment"] = Color.FromRgb(93, 130, 221),
            ["String"] = Color.FromRgb(177, 233, 98),
            ["Offset"] = Color.FromRgb(242, 248, 104),
            ["Instruction"] = Color.FromRgb(93, 181, 222),
            ["Number"] = Color.FromRgb(232, 97, 153),
            ["BuiltInTypes"] = Color.FromRgb(150, 150, 150)
        };

        public Dictionary<string, Color> LightTheme = new Dictionary<string, Color> {
            ["Comment"] = Color.FromRgb(171, 125, 42),
            ["String"] = Color.FromRgb(35, 145, 63),
            ["Offset"] = Color.FromRgb(128, 35, 145),
            ["Instruction"] = Color.FromRgb(171, 124, 41),
            ["Number"] = Color.FromRgb(163, 168, 41),
            ["BuiltInTypes"] = Color.FromRgb(88, 88, 88)
        };

        public MyControl() {
            InitializeComponent();
            this._viewModel = new MyControlVM();
            this.DataContext = this._viewModel;
            this.SetColorTheme();
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) {
            this.SetColorTheme();
        }

        private void SetColorTheme() {
            VisualStudioTheme theme;
            var high = Helpers.GetILHighlightingDefinition();
            if (Common.Instance.Options.ColorTheme != MsilerColorTheme.Auto) {
                theme = Common.Instance.Options.ColorTheme == MsilerColorTheme.Dark
                    ? VisualStudioTheme.Dark
                    : VisualStudioTheme.Light;
            } else {

                theme = VSThemeDetector.GetTheme();
                if (theme == VisualStudioTheme.Unknown) {
                    theme = VisualStudioTheme.Light; // if unknown
                }
            }
            if (theme == VisualStudioTheme.Light || theme == VisualStudioTheme.Blue) {
                this.ApplyTheme(high, LightTheme);
            } else {
                this.ApplyTheme(high, DarkTheme);
            }
            InstructionList.SyntaxHighlighting = high;
        }

        private void ApplyTheme(IHighlightingDefinition def, Dictionary<string, Color> theme) {
            foreach (var kv in theme) {
                def.GetNamedColor(kv.Key).Foreground = new SimpleHighlightingBrush(kv.Value);
            }
        }

        private void InstructionList_MouseHover(object sender, System.Windows.Input.MouseEventArgs e) {
            var pos = InstructionList.GetPositionFromPoint(e.GetPosition(InstructionList));

            if (pos == null)
                return;

            int off = InstructionList.Document.GetOffset(pos.Value.Line, pos.Value.Column);
            var startOff = InstructionList.Document.GetLineByOffset(off).Offset;
            var endOff = InstructionList.Document.GetLineByOffset(off).EndOffset;
            var lineText = InstructionList.Document.GetText(startOff, endOff - startOff);

            var regMatch = lineRegex.Match(lineText);
            if (!regMatch.Success) {
                return;
            }

            string instruction = regMatch.Groups["Instruction"].Value.ToLower();

            if (!Instructions.Description.ContainsKey(instruction)) {
                return;
            }

            toolTip.PlacementTarget = this;
            toolTip.Content = new ICSharpCode.AvalonEdit.TextEditor {
                Text = $"{instruction}: {Instructions.Description[instruction]}",
                Opacity = 0.6,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            };
            toolTip.IsOpen = true;
            e.Handled = true;
        }

        private void InstructionList_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e) {
            toolTip.IsOpen = false;
        }

        private void HyperlinkOptions_Click(object sender, System.Windows.RoutedEventArgs e) {
            Common.Instance.Package.ShowOptionPage(typeof(MsilerOptions));
        }

        private void HyperlinkGithub_Click(object sender, System.Windows.RoutedEventArgs e) {
            System.Diagnostics.Process.Start(RepoUrl);
        }
    }
}