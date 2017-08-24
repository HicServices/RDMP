using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using ReusableLibraryCode;

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

            string doNotBundle = "SimpleStringValueEncryption.cs";

            
            DirectoryInfo d = new DirectoryInfo(args[0]);
            var files = d.EnumerateFiles("*.cs", SearchOption.AllDirectories).Where(f => !f.Name.Contains("TemporaryGeneratedFile")).ToArray();

            var uniqueFileInfos = files.Where(
                f => files.Count(f2 => f2.Name.Equals(f.Name)) == 1 && //where it is a unique file - not 2 files with the same name
                        !f.Name.Equals(doNotBundle)).ToArray();//and its not a thrown out one

            DirectoryInfo outputDirectory = new DirectoryInfo(args[1]);

            DirectoryInfo workingDirectory = Directory.CreateDirectory(Path.Combine(outputDirectory.FullName,"SourceCodeForSelfAwareness"));

            //copy the files to the self awareness folder
            foreach (FileInfo f in uniqueFileInfos)
                f.CopyTo(Path.Combine(workingDirectory.FullName, f.Name));

            Cleanser c = new Cleanser(new DirectoryInfo(workingDirectory.FullName));
            c.Cleanse();

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
                        
                        var md51 = UsefulStuff.MD5File(tempFileName1);
                        var md52 = UsefulStuff.MD5File(tempFileName2);

                        File.Delete(tempFileName1);
                        File.Delete(tempFileName2);

                        if (!md51.Equals(md52))
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
