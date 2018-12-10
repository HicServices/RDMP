using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Ticketing;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See Project
    /// </summary>
    public interface IProject:IHasDependencies, INamed
    {
        /// <summary>
        /// Optional ticket identifier for auditing time, project requirements etc.  Should be compatible with your currently configured <see cref="ITicketingSystem"/>
        /// </summary>
        string MasterTicket { get; set; }

        /// <summary>
        /// Location on disk that the extracted artifacts for the project (csv files , <see cref="SupportingDocument"/> etc) are put in.
        /// </summary>
        string ExtractionDirectory { get; set; }

        /// <summary>
        /// The number you want to associate with Project, a Project is not in a legal state if it doesn't have one (you can't upload cohorts, do extradctions etc).
        /// You can have multiple <see cref="IProject"/> with the same number (in which case they will have shared access to the same cohorts, anonymisation mappings etc).
        /// </summary>
        int? ProjectNumber { get; set; }
        
        /// <summary>
        /// A <see cref="IProject"/> can have multiple <see cref="IExtractionConfiguration"/> defined (e.g. Cases / Controls or multiple extractions over time).  This
        /// returns all current and frozen (released) configurations.
        /// </summary>
        IExtractionConfiguration[] ExtractionConfigurations { get; }

        IEnumerable<IDataUser> DataUsers { get; }
        
        IDataExportRepository DataExportRepository { get; }
        ICatalogue[] GetAllProjectCatalogues();
        ExtractionInformation[] GetAllProjectCatalogueColumns(ExtractionCategory any);
    }
}