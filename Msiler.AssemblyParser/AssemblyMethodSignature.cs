using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Msiler.AssemblyParser
{
    public class AssemblyMethodSignature
    {
        private static readonly Regex GenericFlagRegex = new Regex(@"`\d+", RegexOptions.Compiled);
        private List<string> ParameterTypes { get; }

        private readonly string fullSignature;

        public string MethodName { get; }

        public AssemblyMethodSignature(string methodName, List<string> parameterTypes)
        {
            this.MethodName = ProcessMethodName(methodName);
            this.ParameterTypes = parameterTypes.Select(ProcessParameterType).ToList();
            this.fullSignature = $"{this.MethodName}({String.Join(",", this.ParameterTypes)})";
        }

        private static string ProcessMethodName(string methodName) => GenericFlagRegex.Replace(methodName, String.Empty);

        private static string ProcessParameterType(string parameterType)
        {
            // remove spaces from type name (ex. Func`1<T, U> -> Func`1<T,U>)
            string t = parameterType.Replace(" ", String.Empty);
            // remove generic flags (ex. Func`1<T,U> -> Func<T,U>)
            return GenericFlagRegex.Replace(t, String.Empty);
        }

        public static AssemblyMethodSignature FromMethodDef(MethodDef definition)
        {
            var type = definition.DeclaringType;
            IEnumerable<Parameter> paramList = definition.Parameters;

            // skip implicit this
            if (definition.MethodSig.HasThis)
                paramList = paramList.Skip(1);

            string methodName = $"{GetTypeFullNameFromType(type.ToTypeSig())}.{definition.Name}";
            var paramTypesList = paramList.Select(p => GetTypeFullNameFromType(p.Type)).ToList();
            return new AssemblyMethodSignature(methodName, paramTypesList);
        }

        private static string GetTypeFullNameFromType(TypeSig typeSig)
        {
            string fullName = typeSig.FullName;
            var typeDef = typeSig.TryGetTypeDef();
            if (typeDef != null && typeDef.IsNested)
                fullName = fullName.ReplaceLastOccurrence("/", ".");
  
            if (typeSig.IsSingleOrMultiDimensionalArray)
                fullName = typeSig.ReflectionFullName.ReplaceLastOccurrence("+", ".");
  
            return fullName;
        }

        #region Object overrides

        public override string ToString() => this.fullSignature;

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
                return false;
            var other = (AssemblyMethodSignature)obj;
            return this.fullSignature == other.fullSignature;
        }

        public override int GetHashCode() => this.fullSignature.GetHashCode();

        #endregion
    }
}
