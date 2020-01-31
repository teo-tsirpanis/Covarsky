// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covarsky
{
    // Pun intended.
    public class CovarskyAnalysis : Task
    {
        [Required] public string AssemblyPath { get; set; }
        public string? OutputPath { get; set; }

        public void DoExecute()
        {
            var outputPath = OutputPath ?? AssemblyPath;
            // Better not immediately write to the output file.
            // Properly configuring it is quite complicated.
            using var resultingAssembly = new MemoryStream();
            using (var asm = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                AssemblyRewriter.RewriteAssembly(asm, Log);
                asm.Write(resultingAssembly);
            }

            if (Log.HasLoggedErrors) return;
            using var outputFile = File.Create(outputPath);
            resultingAssembly.CopyTo(outputFile);
        }

        public override bool Execute()
        {
            try
            {
                DoExecute();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }
    }
}