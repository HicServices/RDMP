using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data.Referencing;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.ImportExport
{
    /// <summary>
    /// Identifies an object in the local Catalogue database (or DataExport database) which has been shared externally (via it's SharingUID).  The use of a SharingUID
    /// allows multiple external users to access and import the shared object (and any dependant objects).  Having an ObjectExport declared on an object prevents it from
    /// being deleted (see ObjectSharingObscureDependencyFinder) since this would leave external users with orphaned objects.
    /// </summary>
    public class ObjectExport : ReferenceOtherObjectDatabaseEntity
    {
        #region Database Properties

        private string _sharingUID;
        
        #endregion

        public string SharingUID
        {
            get { return _sharingUID; }
            set { SetField(ref _sharingUID, value); }
        }
        
        /// <inheritdoc cref="SharingUID"/>
        [NoMappingToDatabase]
        public Guid SharingUIDAsGuid { get { return Guid.Parse(SharingUID); }}

        /// <summary>
        /// use <see cref="ShareManager.GetNewOrExistingExportFor"/> for easier access to this constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="objectForSharing"></param>
        internal ObjectExport(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectForSharing, Guid guid)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"ReferencedObjectID",objectForSharing.ID},
                {"ReferencedObjectType",objectForSharing.GetType().Name},
                {"ReferencedObjectRepositoryType",objectForSharing.Repository.GetType().Name},
                {"SharingUID",guid.ToString()},
            
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        public ObjectExport(IRepository repository, DbDataReader r)
            : base(repository, r)
        {
            SharingUID = r["SharingUID"].ToString();
        }

        public override string ToString()
        {
            return "E::" + ReferencedObjectType +"::" + SharingUID;
        }

        /// <summary>
        /// Returns true if this ObjectExport is an export declaration for the passed parameter
        /// </summary>
        /// <returns></returns>
        public bool IsExportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return o.ID == ReferencedObjectID && o.GetType().Name == ReferencedObjectType && o.Repository.GetType().Name == ReferencedObjectRepositoryType;
        }

        /// <summary>
        /// Returns the local object referenced by this export declaration
        /// </summary>
        /// <param name="repositoryLocator"></param>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable GetLocalObject(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            return repositoryLocator.GetArbitraryDatabaseObject(ReferencedObjectRepositoryType, ReferencedObjectType, ReferencedObjectID);
        }
    }
}