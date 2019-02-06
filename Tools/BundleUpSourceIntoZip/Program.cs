// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace BundleUpSourceIntoZip
{
    class Program
    {
        static int Main(string[] args)
        {
            for (int index = 0; index < args.Length; index++)
            {
                Console.WriteLine("Argument "+index +" was :"+ args[index]);
            }


            if (args.Length == 1)
                args = Regex.Split(args[0], "\"(.*)\"[ ,]");

            if (args.Length != 2)
            {
                Console.WriteLine("Please provide 2 arguments, arg1= root directory, arg2= output directory");
                Console.WriteLine("Example Usage: " + AppDomain.CurrentDomain.FriendlyName+@" c:\bob c:\bob\Debug\bin");
                
                return 1;
            }
            
            DirectoryInfo d = new DirectoryInfo(args[0]);
            var files = d.EnumerateFiles("*.cs", SearchOption.AllDirectories).Where(f => !f.FullName.Contains("CodeTutorials")).ToArray();

            var uniqueFileInfos = files.Where(
                f => 
                    files.Count(f2 => f2.Name.Equals(f.Name)) == 1  //where it is a unique file - not 2 files with the same name
                        ).ToArray();

            DirectoryInfo outputDirectory = new DirectoryInfo(args[1]);

            DirectoryInfo workingDirectory = Directory.CreateDirectory(Path.Combine(outputDirectory.FullName,"SourceCodeForSelfAwareness"));

            //copy the files to the self awareness folder
            foreach (FileInfo f in uniqueFileInfos)
                f.CopyTo(Path.Combine(workingDirectory.FullName, f.Name));

            string plannedZipFileName = Path.Combine(outputDirectory.FullName, "SourceCodeForSelfAwareness.zip");

            //if there is an old zip file
            if (File.Exists(plannedZipFileName))
            {
                //if there is a failed .new file from a previous crash, clean it up
                if (File.Exists(plannedZipFileName + ".new"))
                    File.Delete(plannedZipFileName + ".new");

                //create a .zip.new version
                ZipFile.CreateFromDirectory(workingDirectory.FullName, plannedZipFileName + ".new");

                //see if the .new is different from the .zip
                if (DiffZipArchives.AreSame(plannedZipFileName, plannedZipFileName + ".new"))
                    File.Delete(plannedZipFileName + ".new"); //delete the newly created one
                else
                {
                    File.Delete(plannedZipFileName);
                    File.Move(plannedZipFileName + ".new", plannedZipFileName);
                        //they are different so delete the one sat on disk and rename .zip.new to .zip
                }
            }
            else
                ZipFile.CreateFromDirectory(workingDirectory.FullName, plannedZipFileName);

            workingDirectory.Delete(true);

            return 0;
      
        }
    }
    
    internal class DiffZipArchives
    {
        public static bool AreSame(string zipFilename1, string zipFilename2)
        {
            using (var zipArchive1 = ZipFile.OpenRead(zipFilename1))
            {
                using (var zipArchive2 = ZipFile.OpenRead(zipFilename2))
                {
                    var entries1 = zipArchive1.Entries.ToArray();
                    var entries2 = zipArchive2.Entries.ToArray();

                    for (int index = 0; index < zipArchive1.Entries.Count; index++)
                    {
                        var e1 = entries1[index];
                        var e2 = entries2[index];

                        //don't have same name
                        if (!e1.Name.Equals(e2.Name))
                            return false;

                        string tempFileName1 = Guid.NewGuid().ToString();
                        string tempFileName2 = Guid.NewGuid().ToString();

                        e1.ExtractToFile(tempFileName1);
                        e2.ExtractToFile(tempFileName2);
                        
                        var md51 = MD5File(tempFileName1);
                        var md52 = MD5File(tempFileName2);

                        File.Delete(tempFileName1);
                        File.Delete(tempFileName2);

                        if (!md51.Equals(md52))
                            return false;
                    }
                }
            }
            
            return true;
        }
        public static string MD5File(string filename, int retryCount = 6)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream));
                    }
                }
            }
            catch (IOException ex)
            {
                //couldn't access the file so wait 5 seconds then try again
                Thread.Sleep(1000);

                if (retryCount-- > 0)
                    return MD5File(filename, retryCount);//try it again (recursively)
                else
                    throw ex;
            }
        }
    }
}
