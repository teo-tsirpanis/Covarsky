// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using Mono.Cecil;
using Sigourney;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covarsky
{
    public class CovarskyTask : MSBuildWeaver
    {
        public string? CustomInAttributeName { get; set; }

        public string? CustomOutAttributeName { get; set; }

        protected override bool DoWeave(AssemblyDefinition asm) =>
            AssemblyRewriter.DoWeave(asm, Log2, CustomOutAttributeName, CustomInAttributeName);
    }
}
