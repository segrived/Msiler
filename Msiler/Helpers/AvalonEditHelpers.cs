using ICSharpCode.AvalonEdit.Document;
using System;

namespace Msiler.Helpers
{
    public static class AvalonEditHelpers
    {
        public static string GetWordOnOffset(ITextSource textSource, int offset) {
            int startOffset, endOffset;

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
            startOffset = processingOffset + 1;

            processingOffset = offset;
            while (processingOffset < textSource.TextLength) {
                var c = textSource.GetCharAt(processingOffset);
                if (Char.IsWhiteSpace(c)) {
                    break;
                }
                processingOffset++;
            }
            endOffset = processingOffset;

            return textSource.GetText(startOffset, endOffset - startOffset);
        }
    }
}
