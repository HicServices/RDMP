namespace ReusableLibraryCode.VisualStudioSolutionFileProcessing
{
    /// <summary>
    /// A csproj file referenced by a .sln file (See VisualStudioSolutionFile)
    /// </summary>
    public class VisualStudioProjectReference
    {
        public string Guid;
        public string Path;
        public string Name;

        public VisualStudioProjectReference(string name, string path, string guid)
        {
            Name = name.Trim();
            Path = path.Trim();
            Guid = guid.Trim();
        }
    }
}