using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Create a resolver for when assemblies cannot be properly loaded through the usual mechanism
    /// and the bidingredirect is not available.
    /// </summary>
    public class AssemblyResolver
    {
        private static HashSet<string> assemblyResolveAttempts = new HashSet<string>(); 

        public static void SetupAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
            {
                string assemblyInfo = resolveArgs.Name;
                var parts = assemblyInfo.Split(',');
                string name = parts[0];

                if (assemblyResolveAttempts.Contains(assemblyInfo))
                    return null;

                assemblyResolveAttempts.Add(assemblyInfo);

                var assembly = Assembly.GetExecutingAssembly().Location;
                if (String.IsNullOrWhiteSpace(assembly))
                    return null;

                var directoryInfo = new FileInfo(assembly).Directory;
                if (directoryInfo == null)
                    return null;

                var file = directoryInfo.EnumerateFiles(name + ".dll").FirstOrDefault();
                if (file == null)
                    return null;

                return Assembly.LoadFile(file.FullName);
            };
        } 
    }
}