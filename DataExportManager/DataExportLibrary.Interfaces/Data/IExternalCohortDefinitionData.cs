using System;

namespace DataExportLibrary.Interfaces.Data
{
    /// <summary>
    /// See ExternalCohortDefinitionData
    /// </summary>
    public interface IExternalCohortDefinitionData
    {
        int ExternalProjectNumber { get; set; }
        string ExternalDescription { get; set; }
        int ExternalVersion { get; set; }
        string ExternalCohortTableName { get; set; }
        DateTime? ExternalCohortCreationDate { get; set; }
    }
}