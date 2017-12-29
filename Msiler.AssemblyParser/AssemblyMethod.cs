using System;
using System.Linq;
using dnlib.DotNet;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace Msiler.AssemblyParser
{
    public class AssemblyMethod
    {
        private static readonly char[] DisallowedMethodNameChars = { '<', '>' };

        public AssemblyMethodSignature Signature { get; }
        public List<Instruction> Instructions { get; }
        public MethodDef MethodDefinition { get; }

        internal AssemblyMethod(MethodDef definition)
        {
            this.MethodDefinition = definition;
            this.Signature = AssemblyMethodSignature.FromMethodDef(definition);
            this.Instructions = this.MethodDefinition.Body.Instructions.ToList();
        }

        public bool IsConstructor => this.MethodDefinition.IsConstructor;
        public bool IsProperty => this.MethodDefinition.IsGetter || this.MethodDefinition.IsSetter;
        public bool IsAnonymous => this.Signature.MethodName.Any(DisallowedMethodNameChars.Contains);

        public string GenerateListing(ListingGeneratorOptions options)
        {
            using (var generator = new ListingGenerator(options))
                return generator.GenerateListing(this);
        }

        #region Object overrides

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
                return false;

            var other = (AssemblyMethod)obj;
            return this.Signature.Equals(other.Signature);
        }

        public override int GetHashCode() => this.Signature.GetHashCode();

        public override string ToString() => this.Signature.ToString();

        #endregion
    }
}
