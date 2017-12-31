using System;
using System.Windows;
using ICSharpCode.AvalonEdit.Document;
using TextEditor = ICSharpCode.AvalonEdit.TextEditor;

namespace Msiler.Lib
{
    public class TextEditorWordProcesor
    {
        private readonly TextEditor editor;
        private Point textPosition;
        private bool needUpdateData;

        private string word;
        private int startOffset;
        private int endOffset;
        private DocumentLine line;
        private bool isValidWord;

        public bool IsValidWord
        {
            get
            {
                this.UpdateDataInRequired();
                return this.isValidWord;
            }
            private set => this.isValidWord = value;
        }

        public DocumentLine Line
        {
            get
            {
                this.UpdateDataInRequired();
                return this.line;
            }
            private set => this.line = value;
        }

        public string Word
        {
            get
            {
                this.UpdateDataInRequired();
                return this.word;
            }
            private set => this.word = value;
        }

        public int StartOffset
        {
            get
            {
                this.UpdateDataInRequired();
                return this.startOffset;
            }
            private set => this.startOffset = value;
        }

        public int EndOffset
        {
            get
            {
                this.UpdateDataInRequired();
                return this.endOffset;
            }
            private set => this.endOffset = value;
        }

        public bool IsOnLineStart => this.StartOffset == this.Line.Offset;

        public TextEditorWordProcesor(TextEditor editor)
        {
            this.editor = editor;
        }

        public void UpdateByPoint(Point p)
        {
            this.textPosition = p;
            this.needUpdateData = true;
        }

        private void SetInvalidWordState()
        {
            this.IsValidWord = false;

            this.Line = null;
            this.StartOffset = this.EndOffset = 0;
            this.Word = String.Empty;
        }

        private void UpdateDataInRequired()
        {
            if (!this.needUpdateData)
                return;

            try
            {
                this.needUpdateData = false;

                var pos = this.editor.GetPositionFromPoint(this.textPosition);

                if (pos == null)
                {
                    this.SetInvalidWordState();
                    return;
                }

                int offset = this.editor.Document.GetOffset(pos.Value.Line, pos.Value.Column);

                this.Line = this.editor.Document.GetLineByOffset(offset);

                if (offset < 0 || offset >= this.editor.Document.TextLength)
                    return;

                if (Char.IsWhiteSpace(this.editor.Document.GetCharAt(offset)))
                    return;

                int processingOffset = offset;
                while (processingOffset >= 0)
                {
                    char c = this.editor.Document.GetCharAt(processingOffset);
                    if (Char.IsWhiteSpace(c))
                        break;
                    processingOffset--;
                }
                int startWordOffset = processingOffset + 1;


                processingOffset = offset;
                while (processingOffset < this.editor.Document.TextLength)
                {
                    char c = this.editor.Document.GetCharAt(processingOffset);
                    if (Char.IsWhiteSpace(c))
                        break;
                    processingOffset++;
                }
                int endWordOffset = processingOffset;

                this.Word = this.editor.Document.GetText(startWordOffset, endWordOffset - startWordOffset);

                this.StartOffset = startWordOffset;
                this.EndOffset = endWordOffset;

                this.IsValidWord = true;
            }
            catch
            {
                this.SetInvalidWordState();
            }
        }
    }
}
