using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Quart.Msiler.Lib;

namespace Quart.Msiler
{
    public class MethodSignatureEventArgs : EventArgs
    {
        public MethodSignature MethodSignature { get; set; }

        public MethodSignatureEventArgs(MethodSignature signature) {
            this.MethodSignature = signature;
        }
    }

    public struct MethodSignature
    {
        public string FullName;
        public List<string> Parameters;

        public MethodSignature(MethodEntity me) {
            this.FullName = me.MethodName;
            this.Parameters = me.ParametersList.ToList();
        }

        public MethodSignature(string fullName, List<string> parameters) {
            this.FullName = fullName;
            this.Parameters = parameters.Select(p => p.Replace(" ", "")).ToList();
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var otherMethod = (MethodSignature)obj;
            // TODO: slow implementation, shoul be fixed
            return this.FullName == otherMethod.FullName
                && this.Parameters.SequenceEqual(otherMethod.Parameters);
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

        protected void OnMethodSelect(MethodSignature methodInfo) {
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

                // should be compitable with Mono.Cecil method names
                string fnName = (ce.Name == me.Name) ? ".ctor" : me.Name;

                var cfParams = ((CodeFunction)me).Parameters;
                var parameterList = new List<string>();
                foreach (CodeParameter param in cfParams) {
                    parameterList.Add(param.Type.AsFullName);
                }
                OnMethodSelect(new MethodSignature($"{ne.Name}.{ce.Name}.{fnName}", parameterList));
            } catch {
                // do nothing
            }
        }
    }
}
