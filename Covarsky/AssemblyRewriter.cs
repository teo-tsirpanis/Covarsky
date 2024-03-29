// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Serilog;

namespace Covarsky
{
    public static class AssemblyRewriter
    {
        private const string ContravariantIn = "ContravariantInAttribute";
        private const string CovariantOut = "CovariantOutAttribute";

        private static TypeDefinition? FindSuitableAttribute(List<TypeDefinition> types, string name, ILogger log)
        {
            var foundAttribute = types.Find(type =>
                // Looks like interfaces do not inherit from Object.
                type.BaseType?.FullName == typeof(Attribute).FullName
                && type.FullName == name);

            switch (foundAttribute)
            {
                case {IsNotPublic: true}:
                    return foundAttribute;
                default:
                    if (foundAttribute != null)
                        log.AttributeIsPublic(foundAttribute.FullName);
                    return null;
            }
        }

        private static bool IsSuitableForVariance(TypeDefinition type) =>
            // Apart from interfaces, the special <Module> class also does not have a base type.
            type.IsInterface || type.BaseType?.FullName == typeof(MulticastDelegate).FullName;

        private static bool ApplyVariance(TypeDefinition type, TypeDefinition? attributeCovariant,
            TypeDefinition? attributeContravariant, ILogger log)
        {
            // Returns if the given generic parameter has an attribute of the given type, and removes it if it does.
            // Removing the attribute class as well is not so simple, and would fail even when it is not used somewhere
            // else where Covarsky doesn't care. Instead, we will just remove the attribute from the generic parameter,
            // and let the IL Linker remove the class if it sees fit.
            static bool CheckAndRemoveAttribute(GenericParameter g, TypeDefinition? attributeType)
            {
                if (attributeType == null) return false;
                var attribute =
                    g.CustomAttributes.SingleOrDefault(attr => attr.AttributeType == attributeType);
                var hasAttribute = attribute != null;
                if (hasAttribute)
                    g.CustomAttributes.Remove(attribute);
                return hasAttribute;
            }

            void PatchGenericIf(bool condition, GenericParameter g, GenericParameterAttributes attr)
            {
                if (!condition) return;
                g.Attributes = (g.Attributes & ~ GenericParameterAttributes.VarianceMask) | attr;
                log.MarkingType(type.FullName, g.Name, attr);
            }

            var hasTypeChanged = false;

            if (!IsSuitableForVariance(type)) return false;
            foreach (var g in type.GenericParameters)
            {
                var shouldBeCovariant = CheckAndRemoveAttribute(g, attributeCovariant);
                var shouldBeContravariant = CheckAndRemoveAttribute(g, attributeContravariant);

                if (!shouldBeCovariant && !shouldBeContravariant) continue;

                if (!g.IsNonVariant)
                {
                    log.TypeIsAlreadyVariant(type.FullName, g.Name);
                    continue;
                }

                if (shouldBeCovariant && shouldBeContravariant)
                {
                    log.CannotDeclareBothVariances(type.FullName, g.Name);
                    continue;
                }

                PatchGenericIf(shouldBeCovariant, g, GenericParameterAttributes.Covariant);
                PatchGenericIf(shouldBeContravariant, g, GenericParameterAttributes.Contravariant);

                hasTypeChanged = true;
            }

            return hasTypeChanged;
        }

        public static bool DoWeave(AssemblyDefinition asm, ILogger log, string? customOutName = null,
            string? customInName = null)
        {
            static string FallbackIfNullOrEmpty(string? str, string fallback) =>
                string.IsNullOrEmpty(str) ? fallback : str!;

            void WarnOnCustomAttribute(string? attrName)
            {
                if (attrName != null) log.CustomAttributeNotFound(attrName);
            }

            var attributeOutName = FallbackIfNullOrEmpty(customOutName, CovariantOut);
            var attributeInName = FallbackIfNullOrEmpty(customInName, ContravariantIn);
            if (attributeInName.Equals(attributeOutName, StringComparison.Ordinal))
            {
                log.AttributeNamesCannotBeTheSame();
                return false;
            }

            var types = asm.Modules.SelectMany(ModuleDefinitionRocks.GetAllTypes).ToList();
            var attributeCovariant = FindSuitableAttribute(types, attributeOutName, log);
            if (attributeCovariant == null)
            {
                WarnOnCustomAttribute(customOutName);
                log.NoCovariantAttributeFound();
            }

            var attributeContravariant = FindSuitableAttribute(types, attributeInName, log);
            if (attributeContravariant == null)
            {
                WarnOnCustomAttribute(customInName);
                log.NoContravariantAttributeFound();
            }

            return (attributeCovariant != null || attributeContravariant != null) && types
                .Select(t => ApplyVariance(t, attributeCovariant, attributeContravariant, log))
                .Aggregate(false, (x, y) => x || y);
        }
    }
}
