using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Windows;

namespace Msiler.Helpers
{
    public class WordBorder
    {
        public int StartOffset { get; }
        public int EndOffset { get; }

        public WordBorder(int startOffset, int endOffset)
        {
            this.StartOffset = startOffset;
            this.EndOffset = endOffset;
        }
    }

    public static class AvalonEditHelpers
    {
        public static string GetWordFromPoint(this TextEditor editor, Point position)
        {
            var pos = editor.GetPositionFromPoint(position);

            if (pos == null)
                return null;

            int offset = editor.Document.GetOffset(pos.Value.Line, pos.Value.Column);

            var wb = GetWordBorders(editor.Document, offset);
            return wb == null ? null : editor.Document.GetText(wb.StartOffset, wb.EndOffset - wb.StartOffset);
        }

        public static WordBorder GetWordBorders(this ITextSource textSource, int offset)
        {
            if (offset < 0 || offset >= textSource.TextLength)
                return null;

            if (Char.IsWhiteSpace(textSource.GetCharAt(offset)))
                return null;

            int processingOffset = offset;
            while (processingOffset >= 0)
            {
                char c = textSource.GetCharAt(processingOffset);
                if (Char.IsWhiteSpace(c))
                    break;
                processingOffset--;
            }

            int startOffset = processingOffset + 1;

            processingOffset = offset;
            while (processingOffset < textSource.TextLength)
            {
                char c = textSource.GetCharAt(processingOffset);
                if (Char.IsWhiteSpace(c))
                    break;
                processingOffset++;
            }

            int endOffset = processingOffset;

            return new WordBorder(startOffset, endOffset);
        }
    }
}
