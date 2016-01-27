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
        public MethodDefinition MethodData { get; set; }
        public List<Instruction> Instructions { get; set; }

        private static Regex genericRegex =
            new Regex(@"`\d+", RegexOptions.Compiled);

        public string MethodName {
            get
            {
                var type = MethodData.DeclaringType;
                var typeName = (type.HasGenericParameters)
                    ? genericRegex.Replace(type.FullName, "")
                    : type.FullName;
                return $"{typeName}.{MethodData.Name}";
            }
        }

        public IEnumerable<string> ParametersList {
            get { return MethodData.Parameters.Select(p => genericRegex.Replace(p.ParameterType.FullName, "")); }
        }

        public string Parameters {
            get { return String.Join(", ", this.ParametersList); }
        }



        public MethodEntity(MethodDefinition methodData, List<Instruction> instructions) {
            MethodData = methodData;
            Instructions = instructions;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var objEntity = (MethodEntity)obj;
            return this.MethodData.FullName == objEntity.MethodData.FullName;
        }

        public override int GetHashCode() {
            return this.MethodData.FullName.GetHashCode();
        }
    }
}
