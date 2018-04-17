using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.ImportExport
{
    /// <summary>
    /// Identifies an object in the local Catalogue database (or DataExport database) which has been shared externally (via it's SharingUID).  The use of a SharingUID
    /// allows multiple external users to access and import the shared object (and any dependant objects).  Having an ObjectExport declared on an object prevents it from
    /// being deleted (see ObjectSharingObscureDependencyFinder) since this would leave external users with orphaned objects.
    /// </summary>
    public class ObjectExport : DatabaseEntity
    {
        #region Database Properties

        private string _objectTypeName;
        private int _objectID;
        private string _sharingUID;
        private string _repositoryTypeName;
        #endregion

        public string ObjectTypeName
        {
            get { return _objectTypeName; }
            set { SetField(ref _objectTypeName, value); }
        }
        public int ObjectID
        {
            get { return _objectID; }
            set { SetField(ref _objectID, value); }
        }
        public string SharingUID
        {
            get { return _sharingUID; }
            set { SetField(ref _sharingUID, value); }
        }
        public string RepositoryTypeName
        {
            get { return _repositoryTypeName; }
            set { SetField(ref _repositoryTypeName, value); }
        }

        /// <inheritdoc cref="SharingUID"/>
        [NoMappingToDatabase]
        public Guid SharingUIDAsGuid { get { return Guid.Parse(SharingUID); }}

        /// <summary>
        /// use CatalogueRepository.GetExportFor to access this constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="objectForSharing"></param>
        internal ObjectExport(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectForSharing, Guid guid)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"ObjectID",objectForSharing.ID},
                {"ObjectTypeName",objectForSharing.GetType().Name},
                {"RepositoryTypeName",objectForSharing.Repository.GetType().Name},
                {"SharingUID",guid.ToString()},
            
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        public ObjectExport(IRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ObjectTypeName = r["ObjectTypeName"].ToString();
            ObjectID = Convert.ToInt32(r["ObjectID"]);
            SharingUID = r["SharingUID"].ToString();
            RepositoryTypeName = r["RepositoryTypeName"].ToString();
        }

        public override string ToString()
        {
            return "E::" + ObjectTypeName +"::" + SharingUID;
        }

        /// <summary>
        /// Returns true if this ObjectExport is an export declaration for the passed parameter
        /// </summary>
        /// <returns></returns>
        public bool IsExportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return o.ID == ObjectID && o.GetType().Name == ObjectTypeName && o.Repository.GetType().Name == RepositoryTypeName;
        }

        /// <summary>
        /// Returns the local object referenced by this export declaration
        /// </summary>
        /// <param name="repositoryLocator"></param>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable GetLocalObject(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            return repositoryLocator.GetArbitraryDatabaseObject(RepositoryTypeName, ObjectTypeName, ObjectID);
        }
    }
}