using System.Collections.Generic;

namespace ReusableLibraryCode.VisualStudioSolutionFileProcessing
{
    /// <summary>
    /// A solution folder defined in a .sln file (See VisualStudioSolutionFile)
    /// </summary>
    public class VisualStudioSolutionFolder
    {
        public string Guid;
        public string Name;

        public VisualStudioSolutionFolder(string name, string guid)
        {
            Name = name.Trim();
            Guid = guid.Trim();
        }

        public List<VisualStudioSolutionFolder> ChildrenFolders = new List<VisualStudioSolutionFolder>();
        public List<VisualStudioProjectReference> ChildrenProjects = new List<VisualStudioProjectReference>();
    }
}

