using System;
using DataExportLibrary.Data;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Exceptions
{
    /// <summary>
    /// Thrown when a given <see cref="IProject"/> doesn't have a <see cref="IProject.ProjectNumber"/> configured yet (null) or that number
    /// did not match an expected value (e.g. <see cref="ExternalCohortDefinitionData.ExternalProjectNumber"/>).
    /// </summary>
    public class ProjectNumberException : Exception
    {
        public ProjectNumberException(string s):base(s)
        {
            
        }
    }
}