using System;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// Record of a single component extracted as part of an <see cref="IExtractionConfiguration"/>.  This could be an anonymised dataset or bundled supporting
    /// documents e.g. Lookups , pdfs etc.  This audit is used to perform release process (where all extracted artifacts are collected and sent somewhere).
    /// </summary>
    public interface IExtractionResults : IMapsDirectlyToDatabaseTable, ISaveable
    {
        string DestinationDescription { get; }
        string DestinationType { get; }
        int RecordsExtracted { get; }
        string Exception { get; set; }
        string SQLExecuted { get; }

        Type GetDestinationType();
        Type GetExtractedType();
    }
}