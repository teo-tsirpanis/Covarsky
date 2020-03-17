// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.Linq;
using System.Reflection;

namespace Covarsky
{
    internal static class Utils
    {
        internal static readonly string CovarskyVersion;

        static Utils()
        {
            var asm = Assembly.GetExecutingAssembly();

            CovarskyVersion = asm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                                  .Cast<AssemblyInformationalVersionAttribute>()
                                  .SingleOrDefault()?.InformationalVersion ?? asm.GetName().Version.ToString();
        }
    }
}