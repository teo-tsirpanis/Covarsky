// Copyright (c) 2021 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using Mono.Cecil;
using Serilog;
using Serilog.Sinks.MSBuild;

// ReSharper disable LogMessageIsSentenceProblem

namespace Covarsky
{
    /// <summary>
    /// Extension methods for Serilog's <see cref="ILogger"/> that record Covarsky's log events.
    /// </summary>
    internal static class LogMessageExtensions
    {
        private static ILogger WithMsBuildCode(this ILogger logger, int code) => logger
            .ForContext(MSBuildProperties.Subcategory, nameof(Covarsky))
            .ForContext(MSBuildProperties.MessageCode, $"COVARSKY01{code:00}");

        public static void AttributeNamesCannotBeTheSame(this ILogger logger) =>
            logger
                .WithMsBuildCode(1)
                .Error("The names of Covarsky's attributes cannot be the same.");

        public static void CustomAttributeNotFound(this ILogger logger, string attributeName) =>
            logger
                .WithMsBuildCode(2)
                .Warning("Custom attribute {AttributeName} was not found.", attributeName);

        public static void TypeIsAlreadyVariant(this ILogger logger, string typeName, string genericParameterName) =>
            logger
                .WithMsBuildCode(3)
                .Warning(
                    "Type {TypeName}'s parameter {GenericParameterName} is already variant and Covarsky will not change it.",
                    typeName, genericParameterName);

        public static void CannotDeclareBothVariances(this ILogger logger, string typeName,
            string genericParameterName) =>
            logger
                .WithMsBuildCode(4)
                .Error(
                    "Type {TypeName}'s parameter {GenericParameterName} cannot be declared as both covariant and contravariant.",
                    typeName, genericParameterName);

        public static void AttributeIsPublic(this ILogger logger, string attributeName) =>
            logger
                .WithMsBuildCode(5)
                .Warning("Attribute {AttributeName} will be ignored because it is public.", attributeName);

        public static void MarkingType(this ILogger logger, string typeName, string genericParameterName,
            GenericParameterAttributes varianceType) =>
            logger
                .Information("Marking type {TypeName}'s parameter {GenericParameterName} as {VarianceType}.",
                    typeName, genericParameterName, varianceType);

        public static void NoCovariantAttributeFound(this ILogger logger) =>
            logger
                .Debug("No suitable attribute for marking covariant parameters was found.");

        public static void NoContravariantAttributeFound(this ILogger logger) =>
            logger
                .Debug("No suitable attribute for marking contravariant parameters was found.");
    }
}
