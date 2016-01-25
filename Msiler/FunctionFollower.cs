using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Quart.Msiler
{
    public class FunctionEventArgs : EventArgs
    {
        public string FunctionName { get; set; }

        public FunctionEventArgs(string functionName) {
            this.FunctionName = functionName;
        }
    }

    public delegate void FunctionSelectedHandler(object sender, FunctionEventArgs e);

    public class FunctionFollower
    {
        private ITextView _view;

        public FunctionFollower(ITextView view) {
            this._view = view;
            this._view.Caret.PositionChanged += Caret_PositionChanged;
        }

        public static event FunctionSelectedHandler FunctionSelected;

        protected void OnFunctionSelect(string functionName) {
            FunctionSelected?.Invoke(this, new FunctionEventArgs(functionName));
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
            TextSelection sel = (TextSelection)doc.Selection;
            if (sel == null) {
                return;
            }

            try {
                FileCodeModel2 fcm = (FileCodeModel2)doc.ProjectItem.FileCodeModel;
                var aPoint = sel.ActivePoint;
                var ne = fcm.CodeElementFromPoint(aPoint, vsCMElement.vsCMElementNamespace);
                var ce = fcm.CodeElementFromPoint(aPoint, vsCMElement.vsCMElementClass);
                var me = fcm.CodeElementFromPoint(aPoint, vsCMElement.vsCMElementFunction);
                OnFunctionSelect($"{ne.Name}.{ce.Name}.{me.Name}");
            } catch {
                // do nothing
            }
        }
    }
}
