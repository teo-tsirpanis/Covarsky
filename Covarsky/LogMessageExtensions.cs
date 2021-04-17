// Copyright (c) 2021 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using Mono.Cecil;
using Serilog;
using Serilog.Sinks.MSBuild;

namespace Covarsky
{
    /// <summary>
    /// Extension methods for Serilog's <see cref="ILogger"/> that record Covarsky's log events.
    /// </summary>
    internal static class LogMessageExtensions {
        private static ILogger WithMsBuildCode(this ILogger logger, int code) =>
            logger.ForContext(MSBuildProperties.MessageCode, $"COVARSKY01{code:00}");

        public static void AttributeNamesCannotBeTheSame(this ILogger logger) =>
            logger
                .WithMsBuildCode(1)
                .Error("The names of Covarsky's attributes cannot be the same");

        public static void NoCovariantAttributeFound(this ILogger logger) =>
            logger
                .WithMsBuildCode(2)
                .Debug("No suitable attribute for marking covariant parameters was found");

        public static void NoContravariantAttributeFound(this ILogger logger) =>
            logger
                .WithMsBuildCode(3)
                .Debug("No suitable attribute for marking contravariant parameters was found");

        public static void CustomAttributeNotFound(this ILogger logger, string attributeName) =>
            logger
                .WithMsBuildCode(4)
                .Warning("Custom attribute {AttributeName} was not found", attributeName);

        public static void MarkingType(this ILogger logger, string typeName, string genericParameterName,
            GenericParameterAttributes varianceType) =>
            logger
                .WithMsBuildCode(5)
                .Information("Marking type {TypeName}'s parameter {GenericParameterName} as {VarianceType}", typeName, 
                    genericParameterName, varianceType);

        public static void TypeIsAlreadyVariant(this ILogger logger, string typeName, string genericParameterName) =>
            logger
                .WithMsBuildCode(6)
                .Warning("Type {TypeName}'s parameter {GenericParameterName} is already variant and Covarsky will not change it",
                    typeName, genericParameterName);

        public static void CannotDeclareBothVariances(this ILogger logger, string typeName, string genericParameterName) =>
            logger
                .WithMsBuildCode(7)
                .Error("Type {TypeName}'s parameter {GenericParameterName} cannot be declared as both covariant and contravariant",
                    typeName, genericParameterName);
    }
}