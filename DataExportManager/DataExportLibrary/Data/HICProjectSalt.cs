using DataExportLibrary.Interfaces.Data;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Data
{
    /// <summary>
    /// Provides the ProjectNumber as the salt for the data export hashing of columns (See ConfigureHashingAlgorithm)
    /// </summary>
    public class HICProjectSalt : IHICProjectSalt
    {
        private readonly IProject _project;

        public HICProjectSalt(IProject project)
        {
            _project = project;
        }

        public string GetSalt()
        {
            return _project.ProjectNumber.ToString();
        }
    }
}