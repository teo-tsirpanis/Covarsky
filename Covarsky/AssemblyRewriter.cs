// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace Covarsky
{
    public class AssemblyRewriter
    {
        private const string ContravariantIn = "ContravariantInAttribute";
        private const string CovariantOut = "CovariantOutAttribute";

        private readonly AssemblyDefinition _assembly;
        private readonly TaskLoggingHelper? _log;
        private readonly List<TypeDefinition> _types;
        private readonly TypeDefinition? _attributeCovariant;
        private readonly TypeDefinition? _attributeContravariant;

        private AssemblyRewriter(AssemblyDefinition assembly, TaskLoggingHelper? log)
        {
            _assembly = assembly;
            _log = log;
            _types = DiscoverTypes();
            _attributeCovariant = FindSuitableAttribute(CovariantOut);
            _attributeContravariant = FindSuitableAttribute(ContravariantIn);
        }

        private List<TypeDefinition> DiscoverTypes()
        {
            var types = new List<TypeDefinition>();

            void DiscoverNestedTypes(TypeDefinition type)
            {
                types.Add(type);
                foreach (var t in type.NestedTypes)
                    DiscoverNestedTypes(t);
            }

            foreach (var t in _assembly.Modules.SelectMany(m => m.Types)) DiscoverNestedTypes(t);
            return types;
        }

        private TypeDefinition? FindSuitableAttribute(string name) =>
            _types.Find(type =>
                // Looks like interfaces do not inherit from Object.
                type.BaseType?.FullName == typeof(Attribute).FullName
                && type.FullName == name
                && type.IsNotPublic);

        private static bool IsSuitableForVariance(TypeDefinition type)
        {
            return type.IsInterface || type.BaseType.FullName == typeof(MulticastDelegate).FullName;
        }

        private void ApplyVariance(TypeDefinition type)
        {
            bool hasAttribute(GenericParameter g, TypeDefinition? attributeType) =>
                g.CustomAttributes.Any(attr => attr.AttributeType == attributeType);

            void patchGeneric(GenericParameter g, bool doIt, GenericParameterAttributes attr)
            {
                if (doIt)
                    g.Attributes = (g.Attributes & ~ GenericParameterAttributes.VarianceMask) | attr;
            }

            if (!IsSuitableForVariance(type)) return;
            _log?.LogMessage(MessageImportance.Low, "Processing {1}...", type.FullName);
            foreach (var g in type.GenericParameters)
            {
                var isCovariant = hasAttribute(g, _attributeCovariant);
                var isContravariant = hasAttribute(g, _attributeContravariant);

                if (!g.IsNonVariant)
                {
                    _log?.LogWarning("Type {0}'s parameter {1} is already variant and it will be ignored.",
                        type.FullName, g.Name);
                    return;
                }

                if (isCovariant && isContravariant)
                {
                    _log?.LogError("Type {0}'s parameter {1} cannot be declared as both covariant and contravariant.",
                        type.FullName, g.Name);
                    return;
                }

                patchGeneric(g, isCovariant, GenericParameterAttributes.Covariant);
                patchGeneric(g, isContravariant, GenericParameterAttributes.Contravariant);
            }
        }

        public static void RewriteAssembly(AssemblyDefinition asm, TaskLoggingHelper? log)
        {
            var cr = new AssemblyRewriter(asm, log);
            cr._types.ForEach(cr.ApplyVariance);
        }
    }
}