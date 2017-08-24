using System;
using System.IO;

namespace Tests.Common
{
    public class TestDirectoryHelper
    {
        private readonly Type _type;
        public DirectoryInfo Directory { get; private set; }

        public TestDirectoryHelper(Type type)
        {
            _type = type;
        }

        public void SetUp(string rootPath = ".")
        {
            if (Directory != null)
                throw new Exception("You should only call SetUp once");

            var rootDir = new DirectoryInfo(rootPath);
            Directory = rootDir.CreateSubdirectory(_type.FullName);
        }

        public void TearDown()
        {
            if (Directory == null)
                throw new Exception("You have called TearDown without calling SetUp (the directory has not been initialised)");

            Directory.Delete(true);
        }

        public void DeleteAllEntriesInDir()
        {
            foreach (var entry in Directory.EnumerateDirectories())
                entry.Delete(true);

            foreach (var entry in Directory.EnumerateFiles())
                entry.Delete();
        }
    }
}