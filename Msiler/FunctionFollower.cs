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
        private readonly ITextView _view;
        private readonly DTE2 _dte;

        public FunctionFollower(ITextView view) {
            this._view = view;
            this._view.Caret.PositionChanged += this.Caret_PositionChanged;
            this._dte = DteHelpers.GetDte();
        }

        public static bool IsFollowingEnabled { get; set; }

        public static event MethodSelectedHandler MethodSelected;

        private void OnMethodSelect(AssemblyMethodSignature methodInfo) {
            MethodSelected?.Invoke(this, new MethodSignatureEventArgs(methodInfo));
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (!IsFollowingEnabled) {
                return;
            }
            // do not use follow mode if "update listing on if visible" option
            // is enabled and toolbox isn't visible
            if (Common.Instance.GeneralOptions.UpdateListingOnlyIfVisible && !MsilerToolWindow.IsVisible) {
                return;
            }

            var doc = this._dte.ActiveDocument;
            if (doc == null) {
                return;
            }
            // only c# supported at this time
            if (doc.Language != "CSharp" && doc.Language != "Basic") {
                return;
            }

            var sel = (TextSelection)doc.Selection;
            if (sel == null) {
                return;
            }

            var fcm = (FileCodeModel2)doc.ProjectItem.FileCodeModel;
            var signature = DteHelpers.GetSignature(sel.ActivePoint, fcm);
            if (signature != null) {
                this.OnMethodSelect(signature);
            }
        }
    }
}
