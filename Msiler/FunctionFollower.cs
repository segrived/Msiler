using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Msiler.Lib;
using Msiler.AssemblyParser;

namespace Msiler
{
    public class MethodSignatureEventArgs : EventArgs
    {
        public AssemblyMethodSignature MethodSignature { get; set; }

        public MethodSignatureEventArgs(AssemblyMethodSignature signature) {
            this.MethodSignature = signature;
        }
    }

    public delegate void MethodSelectedHandler(object sender, MethodSignatureEventArgs e);

    public class FunctionFollower
    {
        private ITextView _view;

        public FunctionFollower(ITextView view) {
            this._view = view;
            this._view.Caret.PositionChanged += Caret_PositionChanged;
        }

        public static event MethodSelectedHandler MethodSelected;

        protected void OnMethodSelect(AssemblyMethodSignature methodInfo) {
            MethodSelected?.Invoke(this, new MethodSignatureEventArgs(methodInfo));
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) {
            var dte = Helpers.GetDTE();
            var doc = dte.ActiveDocument;
            if (doc == null) {
                return;
            }
            // only c# supported at this time
            if (doc.Language != "CSharp") {
                return;
            }
            var sel = (TextSelection)doc.Selection;
            if (sel == null) {
                return;
            }

            var fcm = (FileCodeModel2)doc.ProjectItem.FileCodeModel;
            var signature = sel.ActivePoint.GetSignature(fcm);
            if (signature != null) {
                OnMethodSelect(signature);
            }
        }
    }
}
