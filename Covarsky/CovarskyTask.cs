// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covarsky
{
    public class CovarskyTask : Task
    {
        [Required] public string? AssemblyPath { get; set; }
        public string? OutputPath { get; set; }
        
        public string? CustomInAttributeName { get; set; }
        
        public string? CustomOutAttributeName { get; set; }

        public void DoExecute()
        {
            var outputPath = OutputPath ?? AssemblyPath;
            var readerParams = new ReaderParameters
            {
                ReadWrite = true
            };
            using var asm = AssemblyDefinition.ReadAssembly(AssemblyPath, readerParams);
            AssemblyRewriter.RewriteAssembly(asm, CustomInAttributeName, CustomOutAttributeName, Log);
            var writerParams = new WriterParameters
            {

            };
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