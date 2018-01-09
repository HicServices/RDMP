using System;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ReleaseLogEntry
    /// </summary>
    public interface IReleaseLogEntry : IDeleteable
    {
        int CumulativeExtractionResults_ID { get; }
        string Username { get; }
        DateTime DateOfRelease { get; }
        string MD5OfDatasetFile { get; }
        string DatasetState { get; }
        string EnvironmentState { get; }
        bool IsPatch { get; }
        string ReleaseFolder { get; }
    }
}