// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Reflection;
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

        public void DoExecute()
        {
            var outputPath = OutputPath ?? AssemblyPath;
            var readerParams = new ReaderParameters
            {
                ReadWrite = true
            };
            using var asm = AssemblyDefinition.ReadAssembly(AssemblyPath, readerParams);
            FindStrongNameKey(KeyOriginatorFile ?? AssemblyPath, asm);
            AssemblyRewriter.RewriteAssembly(asm, CustomInAttributeName, CustomOutAttributeName, Log);
            var writerParams = new WriterParameters
            {
                StrongNameKeyPair = _keyPair
            };
            asm.Name.PublicKey = _publicKey;
            asm.Write(outputPath, writerParams);
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