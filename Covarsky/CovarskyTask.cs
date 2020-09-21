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
    public partial class CovarskyTask : Task
    {
        [Required] public string? AssemblyPath { get; set; }
        public string? OutputPath { get; set; }

        public string? CustomInAttributeName { get; set; }

        public string? CustomOutAttributeName { get; set; }

        public string? IntermediateDirectory { get; set; }

        public string? KeyOriginatorFile { get; set; }

        public string? AssemblyOriginatorKeyFile { get; set; }
        public bool SignAssembly { get; set; } = false;

        /// <summary>
        /// Executes the task: applies co(ntra)variance to the given assembly.
        /// </summary>
        /// <param name="useLog">Setting it to <c>false</c> will disable logging.
        /// Useful for testing scenarios without the full MSBuild behind.</param>
        public void DoExecute(bool useLog = true)
        {
            var outputPath = OutputPath ?? AssemblyPath;
            // Better not write the file if nothing was written in the first place.
            bool shouldSaveNewAssembly;
            using var resultingAsembly = new MemoryStream();
            using (var asm = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                FindStrongNameKey(KeyOriginatorFile ?? AssemblyPath, asm);
                shouldSaveNewAssembly = AssemblyRewriter.RewriteAssembly(asm, CustomInAttributeName,
                    CustomOutAttributeName, useLog ? Log : null);
                var writerParams = new WriterParameters
                {
                    StrongNameKeyPair = _keyPair
                };
                asm.Name.PublicKey = _publicKey;
                asm.Write(resultingAsembly, writerParams);
            }

            if (Log.HasLoggedErrors || !shouldSaveNewAssembly) return;
            using var outputFile = File.Create(outputPath);
            resultingAsembly.Position = 0;
            resultingAsembly.CopyTo(outputFile);
        }

        public override bool Execute()
        {
            DoExecute();
            return !Log.HasLoggedErrors;
        }
    }
}