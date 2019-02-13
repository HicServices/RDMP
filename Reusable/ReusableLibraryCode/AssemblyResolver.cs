// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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