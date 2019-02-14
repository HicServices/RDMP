using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Referencing
{
    /// <summary>
    /// Interface for all objects which reference a single other object and correctly persist it to the RDMP database
    /// </summary>
    public interface IReferenceOtherObjecWithPersist : IReferenceOtherObject
    {
        /// <summary>
        /// The Type of object that was referred to (e.g. <see cref="Catalogue"/>).  Must be an <see cref="IMapsDirectlyToDatabaseTable"/> object
        /// </summary>
        string ReferencedObjectType { get; set; }

        /// <summary>
        /// The ID of the object being refered to by this class
        /// </summary>
        int ReferencedObjectID { get; set; }

        /// <summary>
        /// The platform database which is storing the object being referred to (e.g. DataExport or Catalogue)
        /// </summary>
        string ReferencedObjectRepositoryType { get; }
    }
}