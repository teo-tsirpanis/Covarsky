// Source code incorporated by Fody.
// https://github.com/Fody/Fody/blob/6.1.0/FodyIsolated/StrongNameKeyFinder.cs

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Covarsky
{
    public partial class CovarskyTask
    {
        private StrongNameKeyPair? _keyPair;
        private byte[]? _publicKey;

        private void FindStrongNameKey(string keyPath, AssemblyDefinition asm)
        {
            if (!SignAssembly)
            {
                return;
            }

            var keyFilePath = GetKeyFilePath(keyPath, asm);
            if (keyFilePath == null) return;

            if (!File.Exists(keyFilePath))
                throw new FileNotFoundException("KeyFilePath was defined but file does not exist.", keyFilePath);

            var fileBytes = File.ReadAllBytes(keyFilePath);
            _keyPair = new StrongNameKeyPair(fileBytes);

            try
            {
                _publicKey = _keyPair.PublicKey;
            }
            catch (ArgumentException)
            {
                _keyPair = null;
                _publicKey = fileBytes;
            }
        }

        private string? GetKeyFilePath(string keyFilePath, AssemblyDefinition asm)
        {
            if (keyFilePath != null)
            {
                keyFilePath = Path.GetFullPath(keyFilePath);
                Log?.LogMessage($"Using strong name key from KeyFilePath '{keyFilePath}'.");
                return keyFilePath;
            }

            var assemblyKeyFileAttribute = asm
                .CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name == "AssemblyKeyFileAttribute");
            if (assemblyKeyFileAttribute != null)
            {
                var keyFileSuffix = (string) assemblyKeyFileAttribute.ConstructorArguments.First().Value;
                keyFilePath = Path.Combine(IntermediateDirectory, keyFileSuffix);
                Log?.LogMessage(
                    $"Using strong name key from [AssemblyKeyFileAttribute(\"{keyFileSuffix}\")] '{keyFilePath}'");
                return keyFilePath;
            }

            Log?.LogMessage("No strong name key found");
            return null;
        }
    }
}