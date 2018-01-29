using System.Collections.Generic;
using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;

namespace DataExportLibrary.Interfaces.ExtractionTime.UserPicks
{
    /// <summary>
    /// See ExtractableDatasetBundle
    /// </summary>
    public interface IExtractableDatasetBundle
    {
        IExtractableDataSet DataSet { get; }
        List<SupportingDocument> Documents { get; }
        List<SupportingSQLTable> SupportingSQL { get; }
        List<IBundledLookupTable> LookupTables { get; }

        void DropContent(object toDrop);

        Dictionary<object, ExtractCommandState> States { get; }
    }
}