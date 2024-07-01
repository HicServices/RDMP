using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Repositories
{
    public class YamlRepositoryPatcher
    {
        private DirectoryInfo _directory;
        private YamlRepository _repository;

        public YamlRepositoryPatcher(DirectoryInfo directory, YamlRepository repository)
        {
            _directory = directory;
            _repository = repository;
        }

        public Version GetYAMLVersion()
        {
            string versionPath = Path.Combine(_directory.FullName, "version.yaml");
            if (File.Exists(versionPath))
            {
                return new Version(File.ReadAllText(versionPath));
            }
            else
            {
                File.WriteAllText(versionPath, _repository.GetVersion().ToString());
                return _repository.GetVersion();
            }
        }

        public bool RequiresPatching(Version currentVersion, Version incomingVersion)
        {
            return incomingVersion > currentVersion;
        }

        public void PatchYamlRepository()
        {
            var currentVersion = GetYAMLVersion();
            var newVersion = _repository.GetVersion();
            var isWindows = OperatingSystem.IsWindows();
            DirectoryInfo yamlPatches = new DirectoryInfo(Path.Join(Environment.CurrentDirectory,"YAMLPatches", isWindows ? "windows" : "linux"));
            var patches = yamlPatches.GetFiles().Where(f => f.Name.EndsWith(isWindows?".bat":".sh")).ToArray();
            Array.Sort(patches, delegate (FileInfo x, FileInfo y)
            { //todo check this sort works as expectted for 01_XXX anmd 02_YYY etc
                return string.Compare(x.Name, y.Name);
            });
            // find all patchers that are > current verion and run them in order
            foreach (FileInfo f in patches)
            {
                using (StreamReader reader = new StreamReader(f.FullName))
                {
                    var firstLine = reader.ReadLine() ?? "";
                    if (Version.TryParse(isWindows?firstLine[2..]:firstLine[1..], out var foundVersion) && foundVersion > currentVersion)
                    {
                        //run the patch
                        var psi = new ProcessStartInfo();
                        psi.FileName = isWindows ? "cmd /C" : "/bin/bash";
                        psi.Arguments = f.FullName;
                        psi.RedirectStandardOutput = true;
                        psi.RedirectStandardError = true;
                        psi.UseShellExecute = false;
                        psi.WorkingDirectory = _directory.FullName;
                        //psi.CreateNoWindow = true;
                        using var process = Process.Start(psi);
                        process.WaitForExit();
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();
                        Console.WriteLine(output);
                    }
                }
            }
        }


    }
}
