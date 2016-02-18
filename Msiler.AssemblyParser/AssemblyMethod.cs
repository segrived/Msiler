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

        public MethodDef MethodDefinition { get; set; }

        internal AssemblyMethod(MethodDef definition) {
            this.MethodDefinition = definition;
            this.Signature = AssemblyMethodSignature.FromMethodDef(definition);
            this.Instructions = MethodDefinition.Body.Instructions.ToList();
        }

        public bool IsConstructor => MethodDefinition.IsConstructor;
        public bool IsProperty => MethodDefinition.IsGetter || MethodDefinition.IsSetter;
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

        public override string ToString() {
            return this.Signature.ToString();
        }

        public string GenerateListing(ListingGeneratorOptions options) {
            using (var generator = new ListingGenerator(options)) {
                return generator.GenerateListing(this);
            }
        }
    }
}
