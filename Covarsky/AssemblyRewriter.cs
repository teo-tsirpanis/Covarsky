// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        private const string ProcessedByCovarsky = "ProcessedByCovarsky";

        private readonly AssemblyDefinition _assembly;
        private readonly TaskLoggingHelper? _log;
        private readonly List<TypeDefinition> _types;
        private readonly TypeDefinition? _attributeCovariant;
        private readonly TypeDefinition? _attributeContravariant;

        private AssemblyRewriter(AssemblyDefinition assembly, string? customInName, string? customOutName,
            TaskLoggingHelper? log)
        {
            _assembly = assembly;
            _log = log;
            _types = DiscoverTypes();
            _attributeCovariant = FindSuitableAttribute(customInName ?? CovariantOut);
            _attributeContravariant = FindSuitableAttribute(customOutName ?? ContravariantIn);
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
            // Apart from interfaces, the special <Module> class also does not have a base type.
            return type.IsInterface || type.BaseType?.FullName == typeof(MulticastDelegate).FullName;
        }

        private bool ApplyVariance(TypeDefinition type)
        {
            static bool hasAttribute(GenericParameter g, TypeDefinition? attributeType) =>
                g.CustomAttributes.Any(attr => attr.AttributeType == attributeType);

            void patchGeneric(GenericParameter g, bool doIt, GenericParameterAttributes attr)
            {
                if (doIt)
                {
                    g.Attributes = (g.Attributes & ~ GenericParameterAttributes.VarianceMask) | attr;
                    _log?.LogMessage(MessageImportance.High, "Marking {0}'s {1} as {2}...",
                        type.FullName, g.Name, attr);
                }
            }

            var hasVarianceChanged = false;

            if (!IsSuitableForVariance(type)) return false;
            foreach (var g in type.GenericParameters)
            {
                var isCovariant = hasAttribute(g, _attributeCovariant);
                var isContravariant = hasAttribute(g, _attributeContravariant);

                if (!g.IsNonVariant)
                {
                    _log?.LogWarning("Type {0}'s parameter {1} is already variant and it will be ignored.",
                        type.FullName, g.Name);
                    return false;
                }

                if (isCovariant && isContravariant)
                {
                    _log?.LogError("Type {0}'s parameter {1} cannot be declared as both covariant and contravariant.",
                        type.FullName, g.Name);
                    return false;
                }

                patchGeneric(g, isCovariant, GenericParameterAttributes.Covariant);
                patchGeneric(g, isContravariant, GenericParameterAttributes.Contravariant);

                hasVarianceChanged |= isCovariant || isContravariant;
            }

            return hasVarianceChanged;
        }

        private bool ShouldRewriteAssembly()
        {
            return !_types.Exists(td => td.FullName == ProcessedByCovarsky);
        }

        private void MarkAsProcessedByCovarsky()
        {
            _log?.LogMessage($"Adding the {ProcessedByCovarsky} class...");
            const TypeAttributes typeAttributes =
                TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed;
            var td = new TypeDefinition("", ProcessedByCovarsky, typeAttributes,
                _assembly.MainModule.TypeSystem.Object);
            
            const FieldAttributes fieldAttributes = FieldAttributes.Assembly |
                                                    FieldAttributes.Literal |
                                                    FieldAttributes.Static |
                                                    FieldAttributes.HasDefault;
            var fieldDefinition = new FieldDefinition("CovarskyVersion", fieldAttributes, _assembly.MainModule.TypeSystem.String)
            {
                Constant = Utils.CovarskyVersion
            };
            td.Fields.Add(fieldDefinition);

            _assembly.MainModule.Types.Add(td);
        }

        /// <summary>
        /// Rewrites the given assembly by making variant the types that must be. 
        /// </summary>
        /// <param name="asm">The Cecil assembly definition to process.</param>
        /// <param name="customInName">A custom name for the attribute that makes types contravariant.
        /// If not specified, defaults to <see cref="ContravariantIn"/>.</param>
        /// <param name="customOutName">A custom name for the attribute that makes types covariant.
        /// If not specified, defaults to <see cref="CovariantOut"/>.</param>
        /// <param name="log">A <see cref="TaskLoggingHelper"/> to communicate logging events to MSBuild.</param>
        /// <returns>Whether the assembly was actually processed, i.e. whether any type was modified, or
        /// the assembly has already been processed by Covarsky. The latter is discovered with a dummy type added
        /// to the resulting assembly.</returns>
        public static bool RewriteAssembly(AssemblyDefinition asm, string? customInName = null,
            string? customOutName = null, TaskLoggingHelper? log = null)
        {
            var cr = new AssemblyRewriter(asm, customInName, customOutName, log);
            if (cr.ShouldRewriteAssembly())
            {
                var wasModified = cr._types.Select(cr.ApplyVariance).Aggregate(false, (x, y) => x || y);
                cr.MarkAsProcessedByCovarsky();
                
                if (!wasModified)
                    log?.LogMessage("{0} was not modified. Skipping writing...", asm.Name.Name);
                return wasModified;
            }
            log?.LogMessage("Skipping {0} as it has been already processed by Covarsky...", asm.Name.Name);
            return false;
        }
    }
}