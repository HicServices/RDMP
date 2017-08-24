using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDMPInstallerBinDirLister
{
    class Program
    {
        private static string rootUserEntered;

        static void Main(string[] args)
        {
            StreamWriter sw = new StreamWriter("binContents.txt");

            Console.WriteLine(@"Enter root path (e.g. E:\HIC.DataManagementPlatform):");
            rootUserEntered = Console.ReadLine();

            if (rootUserEntered != null)
                rootUserEntered = rootUserEntered.TrimEnd('\\');

            DirectoryInfo root = new DirectoryInfo(rootUserEntered);
            ProcessDirectoryRecursively(root,sw);
            sw.Flush();
            sw.Close();


            Console.WriteLine("Finished writing:binContents.txt");
            Console.ReadLine();
        }


        static HashSet<string> filesFound = new HashSet<string>();

        private static void ProcessDirectoryRecursively(DirectoryInfo dir, StreamWriter sw)
        {
            if (dir.Name.Contains("Test"))
                return;

            if (dir.Name == "RDMPInstallerBinDirLister")
                return;

            foreach (var directoryInfo in dir.GetDirectories())
                ProcessDirectoryRecursively(directoryInfo, sw);

            //found a bin directory
            if (dir.Name == "bin")
            {

                var debugDir = dir.GetDirectories("Debug").SingleOrDefault();

                if(debugDir == null)
                {

                    Console.WriteLine("Directory does not contain a Debug directory:" + dir.FullName);
                    return;
                }

                FileInfo[] files = debugDir.GetFiles();

                
                foreach (var file in files)
                {
                    //already seen this file in another directory - assume everything has the same version of the file with the same name
                    if (filesFound.Contains(file.Name))    
                        continue;

                    if(file.Name.Contains("Tests"))
                        continue;

                    //this is what we are trying to output
                    //<File Id="DataExportManager.EXE" Name="DataExportManager.exe" Source="..\DataExportManager2\DataExportManager2\bin\Debug\DataExportManager.exe" />

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<File Id=\"" + file.Name.ToUpper() + "\" ");

                    sb.Append("Name=\"" + file.Name + "\" ");


                    if(!file.FullName.Contains(rootUserEntered))
                        Console.WriteLine("File " + file.FullName + " does not contain the root you entered!: " + rootUserEntered + " make sure you have used the correct capitalization");

                    string relativePath = file.FullName.Replace(rootUserEntered, "..");


                    sb.Append("Source=\"" + relativePath + "\" />");

                    string result = sb.ToString();
                    Console.WriteLine(result);
                    sw.WriteLine(result);
                    sw.Flush();

                    filesFound.Add(file.Name);
                }

            }
        }
    }
}
