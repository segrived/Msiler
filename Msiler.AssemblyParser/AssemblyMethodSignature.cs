using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Msiler.AssemblyParser
{
    public class AssemblyMethodSignature
    {
        private static readonly Regex GenericFlagRegex =
            new Regex(@"`\d+", RegexOptions.Compiled);

        public string MethodName { get; private set; }
        public List<string> ParameterTypes { get; private set; }

        private readonly string _fullSignature;

        public AssemblyMethodSignature(string methodName, List<string> parameterTypes) {
            this.MethodName = this.ProcessMethodName(methodName);
            this.ParameterTypes = parameterTypes.Select(this.ProcessParameterType).ToList();
            this._fullSignature = $"{this.MethodName} ({String.Join(",", this.ParameterTypes)})";
        }

        private string ProcessMethodName(string methodName) {
            methodName = GenericFlagRegex.Replace(methodName, String.Empty);
            return methodName;
        }

        private string ProcessParameterType(string parameterType) {
            // remove spaces from type name (ex. Func`1<T, U> -> Func`1<T,U>)
            var t = parameterType.Replace(" ", String.Empty);
            // remove generic flags (ex. Func`1<T,U> -> Func<T,U>)
            return GenericFlagRegex.Replace(t, String.Empty);
        }

        public static AssemblyMethodSignature FromMethodDef(MethodDef definition) {
            var type = definition.DeclaringType;
            IEnumerable<Parameter> paramList = definition.Parameters;
            // skip implicit this
            if (definition.MethodSig.HasThis) {
                paramList = paramList.Skip(1);
            }
            var methodName = $"{type.FullName}.{definition.Name}";
            var paramTypesList = paramList.Select(p => p.Type.FullName).ToList();
            return new AssemblyMethodSignature(methodName, paramTypesList);
        }

        public override string ToString() => this._fullSignature;

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var other = (AssemblyMethodSignature)obj;
            return this._fullSignature == other._fullSignature;
        }

        public override int GetHashCode() {
            return this._fullSignature.GetHashCode();
        }
    }
}
