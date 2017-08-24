using DataExportLibrary.Interfaces.Data;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Data
{
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