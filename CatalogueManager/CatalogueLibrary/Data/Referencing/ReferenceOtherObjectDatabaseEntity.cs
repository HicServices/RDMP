using System;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Referencing
{
    /// <summary>
    /// Abstract base class for all database objects that reference a single other arbitrary database object e.g. <see cref="Favourite"/>.
    /// </summary>
    public abstract class ReferenceOtherObjectDatabaseEntity : DatabaseEntity, IReferenceOtherObject
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

        /// <summary>
        /// True if the object referenced by this class is of Type <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsReferenceTo(Type type)
        {
            return AreProbablySameType(ReferencedObjectType, type);
        }
        
        /// <summary>
        /// True if the <paramref name="o"/> is the object that is explicitly referenced by this class instance
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool IsReferenceTo(IMapsDirectlyToDatabaseTable o)
        {
            return o.ID == ReferencedObjectID
                   &&
                   AreProbablySameType(ReferencedObjectType, o.GetType())
                   &&
                   AreProbablySameType(ReferencedObjectRepositoryType, o.Repository.GetType());
        }

        private bool AreProbablySameType(string storedTypeName, Type candidate)
        {
            return
                storedTypeName.Equals(candidate.Name, StringComparison.CurrentCultureIgnoreCase) ||
                storedTypeName.Equals(candidate.FullName, StringComparison.CurrentCultureIgnoreCase);
        }
        
        /// <summary>
        /// Returns the instance of the object referenced by this class or null if it no longer exists (e.g. has been deleted)
        /// </summary>
        /// <param name="repositoryLocator"></param>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable GetReferencedObject(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            return repositoryLocator.GetArbitraryDatabaseObject(ReferencedObjectRepositoryType, ReferencedObjectType, ReferencedObjectID);
        }
    }
}