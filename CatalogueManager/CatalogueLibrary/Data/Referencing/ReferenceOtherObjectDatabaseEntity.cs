using System;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Referencing
{
    public abstract class ReferenceOtherObjectDatabaseEntity : DatabaseEntity, IReferenceOtherObjects
    {
        private string _referencedObjectType;
        private int _referencedObjectID;
        private string _referencedObjectRepositoryType;

        /// <summary>
        /// The Type of object that was referred to (e.g. <see cref="Catalogue"/>).  Must be an <see cref="IMapsDirectlyToDatabaseTable"/> object
        /// </summary>
        public string ReferencedObjectType
        {
            get { return _referencedObjectType; }
            set { SetField(ref _referencedObjectType, value); }
        }

        /// <summary>
        /// The ID of the object being refered to by this class
        /// </summary>
        public int ReferencedObjectID
        {
            get { return _referencedObjectID; }
            set { SetField(ref _referencedObjectID, value); }
        }

        /// <summary>
        /// The platform database which is storing the object being referred to (e.g. DataExport or Catalogue)
        /// </summary>
        public string ReferencedObjectRepositoryType
        {
            get { return _referencedObjectRepositoryType; }
            set { SetField(ref _referencedObjectRepositoryType, value); }
        }

        /// <inheritdoc/>
        protected ReferenceOtherObjectDatabaseEntity():base()
        {
            
        }
        /// <inheritdoc/>
        protected ReferenceOtherObjectDatabaseEntity(IRepository repository,DbDataReader r):base(repository,r)
        {
            ReferencedObjectType = r["ReferencedObjectType"].ToString();
            ReferencedObjectID = Convert.ToInt32(r["ReferencedObjectID"]);
            ReferencedObjectRepositoryType = r["ReferencedObjectRepositoryType"].ToString();
        }
    }
}