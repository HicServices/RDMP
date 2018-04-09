using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.ImportExport
{
    /// <summary>
    /// Identifies an object in the local Catalogue database (or DataExport database) which was imported from an external catalogue (See ObjectExport).  The SharingUID
    /// allows you to always identify which local object represents a remoted shared object (e.g. available from a web service).  The remote object will have a different
    ///  ID but the same SharingUID).  Sometimes you will import whole networks of objects which might have shared object dependencies in this case newly imported 
    /// networks will reference existing imported objects where they are already available.
    /// 
    /// <para>This table exists to avoid all the unmaintainability/scalability of IDENTITY INSERT whilst also ensuring referential integrity of object shares and preventing
    /// duplication of imported objects.</para>
    /// </summary>
    public class ObjectImport : DatabaseEntity
    {
        #region Database Properties

        private string _sharingUID;
        private int _localObjectID;
        private string _localTypeName;
        private string _repositoryTypeName;
        #endregion

        public string SharingUID
        {
            get { return _sharingUID; }
            set { SetField(ref _sharingUID, value); }
        }
        public int LocalObjectID
        {
            get { return _localObjectID; }
            set { SetField(ref _localObjectID, value); }
        }
        public string LocalTypeName
        {
            get { return _localTypeName; }
            set { SetField(ref _localTypeName, value); }
        }
        public string RepositoryTypeName
        {
            get { return _repositoryTypeName; }
            set { SetField(ref _repositoryTypeName, value); }
        }
        /// <summary>
        /// Use GetImportAs to access this
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sharingUID"></param>
        /// <param name="localObject"></param>
        internal ObjectImport(ICatalogueRepository repository, string sharingUID,IMapsDirectlyToDatabaseTable localObject)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"LocalTypeName",localObject.GetType().Name},
                {"LocalObjectID",localObject.ID},
                {"SharingUID",sharingUID},
                {"RepositoryTypeName",localObject.Repository.GetType().Name}
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        public ObjectImport(IRepository repository, DbDataReader r)
            : base(repository, r)
        {
            SharingUID = r["SharingUID"].ToString();
            LocalObjectID = Convert.ToInt32(r["LocalObjectID"]);
            LocalTypeName = r["LocalTypeName"].ToString();
            RepositoryTypeName = r["RepositoryTypeName"].ToString();
        }

        public bool IsImportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return o.ID == LocalObjectID && o.GetType().Name.Equals(LocalTypeName) && o.Repository.GetType().Name.Equals(RepositoryTypeName);
        }

        public bool LocalObjectStillExists(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            return repositoryLocator.ArbitraryDatabaseObjectExists(RepositoryTypeName, LocalTypeName, LocalObjectID);
        }

        public IMapsDirectlyToDatabaseTable GetLocalObject(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            return repositoryLocator.GetArbitraryDatabaseObject(RepositoryTypeName, LocalTypeName, LocalObjectID);
        }
    }
}
