using System.Collections.Generic;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface IProject
    {
        string Name { get; set; }
        string MasterTicket { get; set; }
        string ExtractionDirectory { get; set; }
        int? ProjectNumber { get; set; }
        
        IExtractionConfiguration[] ExtractionConfigurations { get; }
        IEnumerable<IDataUser> DataUsers { get; }
        
        IDataExportRepository DataExportRepository { get; }
    }
}