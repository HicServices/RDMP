using System;
using CatalogueLibrary.Data.Referencing;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See SupplementalExtractionResults
    /// </summary>
    public interface ISupplementalExtractionResults : IReferenceOtherObject, IExtractionResults
    {
        int? CumulativeExtractionResults_ID { get; }
        int? ExtractionConfiguration_ID { get; }

        bool IsGlobal { get; }
        
        string ReferencedObjectType { get; set; }
        int ReferencedObjectID { get; set; }
        string ExtractedName { get; }
        string ReferencedObjectRepositoryType { get; }

        void CompleteAudit(Type destinationType, string destinationDescription, int uniqueIdentifiers);
        bool IsReferenceTo(Type type);
    }
}