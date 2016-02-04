using System;
using System.Linq;
using dnlib.DotNet;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace Msiler.AssemblyParser
{
    public class AssemblyMethod
    {
        private readonly static char[] DisallowedMethodNameChars = { '<', '>' };

        public AssemblyMethodSignature Signature { get; private set; }
        public List<Instruction> Instructions { get; set; }

        private MethodDef methodDefinition;

        internal AssemblyMethod(MethodDef definition) {
            this.methodDefinition = definition;
            this.Signature = AssemblyMethodSignature.FromMethodDef(definition);
            this.Instructions = methodDefinition.Body.Instructions.ToList();
        }

        public bool IsConstructor => methodDefinition.IsConstructor;
        public bool IsProperty => methodDefinition.IsGetter || methodDefinition.IsSetter;
        public bool IsAnonymous => this.Signature.MethodName.Any(DisallowedMethodNameChars.Contains);

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var other = (AssemblyMethod)obj;
            return this.Signature.Equals(other.Signature);
        }

        public override int GetHashCode() {
            return this.Signature.GetHashCode();
        }

        public string GenerateListing(ListingGeneratorOptions options) {
            return new ListingGenerator(options).GenerateListing(this);
        }
    }
}
