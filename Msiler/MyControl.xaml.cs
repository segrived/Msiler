using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Quart.Msiler
{
    public partial class MyControl : UserControl
    {
        private readonly MyControlVM _viewModel;

        ToolTip toolTip = new ToolTip();
        Regex lineRegex =
            new Regex(@"^IL_(?<Offset>[a-f\d]+)\s+(?<Instruction>[a-z\d.]+)\s+(?<Operand>.*)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public MyControl() {
            InitializeComponent();
            this._viewModel = new MyControlVM();
            this.DataContext = this._viewModel;
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

            string instruction = regMatch.Groups["Instruction"].Value;

            if (!Instructions.Description.ContainsKey(instruction)) {
                return;
            }

            toolTip.PlacementTarget = this;
            toolTip.Content = new ICSharpCode.AvalonEdit.TextEditor {
                Text = $"{instruction}: {Instructions.Description[instruction]}",
                Opacity = 0.6
            };
            toolTip.IsOpen = true;
            e.Handled = true;
        }

        private void InstructionList_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e) {
            toolTip.IsOpen = false;
        }
    }
}