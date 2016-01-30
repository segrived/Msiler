using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quart.Msiler.Lib
{
    public class MethodEntity
    {
        public MethodSignature Signature { get; set; }
        public List<Instruction> Instructions { get; set; }

        private MethodDefinition _methodData { get; set; }

        private static Regex genericRegex =
            new Regex(@"`\d+", RegexOptions.Compiled);
        private readonly static char[] DisallowedMethodNameChars = { '<', '>' };

        public MethodEntity(MethodDefinition methodData, List<Instruction> instructions) {
            this._methodData = methodData;
            this.Instructions = instructions;
            var fullMethodName = this.ExtractMethodName(methodData);
            var parameterList = this.ExtractMethodParameters(methodData);
            this.Signature = new MethodSignature(fullMethodName, parameterList);
        }

        public bool IsConstructor => _methodData.IsConstructor;
        public bool IsProperty => _methodData.IsGetter || _methodData.IsSetter;
        public bool IsAnonymous => Signature.Name.Any(DisallowedMethodNameChars.Contains);

        #region Helper methods
        private string ExtractMethodName(MethodDefinition definition) {
            var type = definition.DeclaringType;
            var typeName = (type.HasGenericParameters)
                ? genericRegex.Replace(type.FullName, "")
                : type.FullName;
            return $"{typeName}.{definition.Name}";
        }

        private List<string> ExtractMethodParameters(MethodDefinition definition) {
            return definition.Parameters
                // remove generic indicator
                .Select(p => genericRegex.Replace(p.ParameterType.FullName, ""))
                .ToList();
        }
        #endregion

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var objEntity = (MethodEntity)obj;
            return this._methodData.FullName == objEntity._methodData.FullName;
        }

        public override int GetHashCode() {
            return this._methodData.FullName.GetHashCode();
        }
    }
}
