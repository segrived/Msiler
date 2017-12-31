using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.VisualStudio.PlatformUI;
using Msiler.Helpers;

namespace Msiler.Lib
{
    public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private readonly TextEditor editor;
        private SolidColorBrush brush;

        public HighlightCurrentLineBackgroundRenderer(TextEditor editor)
        {
            this.editor = editor;
            this.UpdateBrush();

            VSColorTheme.ThemeChanged += e => this.UpdateBrush();
        }

        public KnownLayer Layer => KnownLayer.Caret;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (this.editor.Document == null)
                return;

            textView.EnsureVisualLines();
            var currentLine = this.editor.Document.GetLineByOffset(this.editor.CaretOffset);

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                double width = textView.ActualWidth - 32;

                if (width < 0)
                    return;

                var drawRect = new Rect(rect.Location, new Size(textView.ActualWidth - 32, rect.Height));
                drawingContext.DrawRectangle(this.brush, null, drawRect);
            }
        }

        #region Utils

        private void UpdateBrush()
        {
            var backColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            var higtlightColor = VsThemeHelpers.GetHightlightColor(backColor, 0x80);
            this.brush = new SolidColorBrush(higtlightColor);
        }

        #endregion
    }

}
