using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quart.Msiler.Lib
{
    public class MethodSignature
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }

        private readonly string strSignature;

        public static MethodSignature FromPoint(FileCodeModel2 fcm, VirtualPoint point) {
            try {
                var ne = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementNamespace);
                var ce = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementClass);
                var me = fcm.CodeElementFromPoint(point, vsCMElement.vsCMElementFunction);

                // should be compitable with Mono.Cecil method names
                string fnName = (ce.Name == me.Name) ? ".ctor" : me.Name;

                var cfParams = ((CodeFunction)me).Parameters;
                var parameterList = new List<string>();
                foreach (CodeParameter param in cfParams) {
                    if (param.Type.TypeKind == vsCMTypeRef.vsCMTypeRefArray) {
                        var arrayType = param.Type.ElementType;
                        var fullType = arrayType.AsFullName + "[]";
                        parameterList.Add(fullType);
                    } else {
                        parameterList.Add(param.Type.AsFullName);
                    }

                }
                return new MethodSignature($"{ne.Name}.{ce.Name}.{fnName}", parameterList);
            } catch {
                return null;
            }
        }

        public MethodSignature(string name, List<string> parameters) {
            this.Name = name;
            this.Parameters = parameters.Select(p => p.Replace(" ", "")).ToList();
            this.strSignature = String.Join(",", this.Parameters);
        }

        public string ParametersString => this.strSignature;

        public override string ToString() {
            return this.strSignature;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var otherMethod = (MethodSignature)obj;
            return this.strSignature == otherMethod.strSignature;
        }

        public override int GetHashCode() {
            return this.strSignature.GetHashCode();
        }
    }
}
