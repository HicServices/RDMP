using System;
using System.Collections.Generic;
using System.IO;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Checks whether two DirectoryInfo objects are the same based on FullName
    /// </summary>
    public class DirectoryInfoComparer : IEqualityComparer<DirectoryInfo>
    {
        public bool Equals(DirectoryInfo x, DirectoryInfo y)
        {
            if (object.ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            return x.FullName == y.FullName;
        }

        public int GetHashCode(DirectoryInfo obj)
        {
            if (obj == null)
                return 0;
            return obj.FullName.GetHashCode();
        }
    }

    public static class DirectoryInfoExtensions
    {
        public static void CopyAll(this DirectoryInfo source, DirectoryInfo target)
        {
            if (String.Equals(source.FullName, target.FullName, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                diSourceSubDir.CopyAll(nextTargetSubDir);
            }
        }
    }
}