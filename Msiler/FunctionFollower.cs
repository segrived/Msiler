using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Msiler.AssemblyParser;
using Msiler.Helpers;

namespace Msiler
{
    public class MethodSignatureEventArgs : EventArgs
    {
        public AssemblyMethodSignature MethodSignature { get; private set; }

        public MethodSignatureEventArgs(AssemblyMethodSignature signature) {
            this.MethodSignature = signature;
        }
    }

    public delegate void MethodSelectedHandler(object sender, MethodSignatureEventArgs e);

    public class FunctionFollower
    {
        ITextView _view;
        DTE2 _dte;

        public FunctionFollower(ITextView view) {
            this._view = view;
            this._view.Caret.PositionChanged += Caret_PositionChanged;
            this._dte = DTEHelpers.GetDTE();
        }

        public static bool IsFollowingEnabled { get; set; } = false;

        public static event MethodSelectedHandler MethodSelected;

        protected void OnMethodSelect(AssemblyMethodSignature methodInfo) {
            MethodSelected?.Invoke(this, new MethodSignatureEventArgs(methodInfo));
        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (!IsFollowingEnabled) {
                return;
            }

            var doc = _dte.ActiveDocument;
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
            var signature = DTEHelpers.GetSignature(sel.ActivePoint, fcm);
            if (signature != null) {
                OnMethodSelect(signature);
            }
        }
    }
}
