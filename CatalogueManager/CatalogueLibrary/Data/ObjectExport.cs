using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
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
        public ObjectExport(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectForSharing)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"ObjectID",objectForSharing.ID},
                {"ObjectTypeName",objectForSharing.GetType().Name},
                {"RepositoryTypeName",objectForSharing.Repository.GetType().Name},
                {"SharingUID",Guid.NewGuid().ToString()},
            
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

        /// <summary>
        /// Returns true if this ObjectExport is an export declaration for the passed parameter
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool IsExportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return o.ID == ObjectID && o.GetType().Name == ObjectTypeName && o.Repository.GetType().Name == RepositoryTypeName;
        }
    }
}