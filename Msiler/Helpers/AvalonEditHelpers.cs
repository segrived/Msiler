using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Windows;

namespace Msiler.Helpers
{
    public static class AvalonEditHelpers
    {
        public static string GetWordOnOffset(TextEditor editor, Point position) {
            var pos = editor.GetPositionFromPoint(position);
            if (pos == null)
                return String.Empty;
            int off = editor.Document.GetOffset(pos.Value.Line, pos.Value.Column);
            return GetWordOnOffset(editor.Document, off);
        }

        public static string GetWordOnOffset(ITextSource textSource, int offset) {
            if (offset < 0 || offset >= textSource.TextLength) {
                return String.Empty;
            }

            if (Char.IsWhiteSpace(textSource.GetCharAt(offset))) {
                return String.Empty;
            }

            var processingOffset = offset;
            while (processingOffset >= 0) {
                var c = textSource.GetCharAt(processingOffset);
                if (Char.IsWhiteSpace(c)) {
                    break;
                }
                processingOffset--;
            }
            int startOffset = processingOffset + 1;

            processingOffset = offset;
            while (processingOffset < textSource.TextLength) {
                var c = textSource.GetCharAt(processingOffset);
                if (Char.IsWhiteSpace(c)) {
                    break;
                }
                processingOffset++;
            }
            int endOffset = processingOffset;

            return textSource.GetText(startOffset, endOffset - startOffset);
        }
    }
}
