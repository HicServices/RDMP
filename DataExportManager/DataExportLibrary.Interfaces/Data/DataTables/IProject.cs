using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See Project
    /// </summary>
    public interface IProject:IMapsDirectlyToDatabaseTable
    {
        string Name { get; set; }
        string MasterTicket { get; set; }
        string ExtractionDirectory { get; set; }
        int? ProjectNumber { get; set; }
        
        IExtractionConfiguration[] ExtractionConfigurations { get; }
        IEnumerable<IDataUser> DataUsers { get; }
        
        IDataExportRepository DataExportRepository { get; }
        ICatalogue[] GetAllProjectCatalogues();
        ExtractionInformation[] GetAllProjectCatalogueColumns(ExtractionCategory any);
    }
}