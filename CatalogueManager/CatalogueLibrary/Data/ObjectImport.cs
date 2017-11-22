using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
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
        public ObjectImport(ICatalogueRepository repository, string sharingUID,IMapsDirectlyToDatabaseTable localObject)
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
    }
}
