// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using ILogger = Serilog.ILogger;

namespace Covarsky
{
    public static class AssemblyRewriter
    {
        private const string ContravariantIn = "ContravariantInAttribute";
        private const string CovariantOut = "CovariantOutAttribute";

        private static TypeDefinition? FindSuitableAttribute(List<TypeDefinition> types, string name) =>
            types.Find(type =>
                // Looks like interfaces do not inherit from Object.
                type.BaseType?.FullName == typeof(Attribute).FullName
                && type.FullName == name
                && type.IsNotPublic);

        private static bool IsSuitableForVariance(TypeDefinition type)
        {
            // Apart from interfaces, the special <Module> class also does not have a base type.
            return type.IsInterface || type.BaseType?.FullName == typeof(MulticastDelegate).FullName;
        }

        private static bool ApplyVariance(TypeDefinition type, TypeDefinition? attributeCovariant,
            TypeDefinition? attributeContravariant, ILogger log)
        {
            static bool HasAttribute(GenericParameter g, TypeDefinition? attributeType) =>
                attributeType != null
                && g.CustomAttributes.Any(attr => attr.AttributeType == attributeType);

            void PatchGeneric(GenericParameter g, bool doIt, GenericParameterAttributes attr)
            {
                if (doIt)
                {
                    g.Attributes = (g.Attributes & ~ GenericParameterAttributes.VarianceMask) | attr;
                    log.Information("Marking {0}'s {1} as {2}...", type.FullName, g.Name, attr);
                }
            }

            var hasVarianceChanged = false;

            if (!IsSuitableForVariance(type)) return false;
            foreach (var g in type.GenericParameters)
            {
                var isCovariant = HasAttribute(g, attributeCovariant);
                var isContravariant = HasAttribute(g, attributeContravariant);

                if (!g.IsNonVariant)
                {
                    log.Warning("Type {0}'s parameter {1} is already variant and it will be ignored.",
                        type.FullName, g.Name);
                    return false;
                }

                if (isCovariant && isContravariant)
                {
                    log.Error("Type {0}'s parameter {1} cannot be declared as both covariant and contravariant.",
                        type.FullName, g.Name);
                    return false;
                }

                PatchGeneric(g, isCovariant, GenericParameterAttributes.Covariant);
                PatchGeneric(g, isContravariant, GenericParameterAttributes.Contravariant);

                hasVarianceChanged |= isCovariant || isContravariant;
            }

            return hasVarianceChanged;
        }

        public static bool DoWeave(AssemblyDefinition asm, ILogger log, string? customOutName = null, string? customInName = null)
        {
            var types = asm.Modules.SelectMany(ModuleDefinitionRocks.GetAllTypes).ToList();
            var attributeCovariant = FindSuitableAttribute(types, customOutName ?? CovariantOut);
            if (attributeCovariant == null)
                log.Debug("No suitable attribute for marking covariant parameters was found.");
            var attributeContravariant = FindSuitableAttribute(types, customInName ?? ContravariantIn);
            if (attributeContravariant == null)
                log.Debug("No suitable attribute for marking contravariant parameters was found");

            return
                types
                    .Select(t => ApplyVariance(t, attributeCovariant, attributeContravariant, log))
                    .Aggregate(false, (x, y) => x || y);
        }
    }
}
