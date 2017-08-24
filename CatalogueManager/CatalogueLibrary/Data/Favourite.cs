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
    /// <summary>
    /// Allows you to store a record of a faviourite database object including all objects in Catalogue and DataExport databases.  The Objects type and ID are stored and then 
    /// whenever a CollectionUI is visible and that object is onscreen a star will appear beside it.  Favourites are stored on a 'per user' basis in the Catalogue database so 
    /// even if you switch computers/change sessions Favourites are preserved. 
    /// </summary>
    public class Favourite:DatabaseEntity
    {
        #region Database Properties
        private string _typeName;
        private int _objectID;
        private string _repositoryTypeName;
        private string _username;
        private DateTime _favouritedDate;

        public string TypeName
        {
            get { return _typeName; }
            set { SetField(ref _typeName, value); }
        }

        public int ObjectID
        {
            get { return _objectID; }
            set { SetField(ref _objectID, value); }
        }

        //Tells you which repository the Favourite object is stored in, either the Catalogue or DataExport database
        public string RepositoryTypeName
        {
            get { return _repositoryTypeName; }
            set { SetField(ref _repositoryTypeName, value); }
        } 

        public string Username
        {
            get { return _username; }
            set { SetField(ref _username , value); }
        }

        public DateTime FavouritedDate
        {
            get { return _favouritedDate; }
            set { SetField(ref _favouritedDate, value); }
        }
        #endregion

        public Favourite(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            TypeName = r["TypeName"].ToString();
            ObjectID = Convert.ToInt32(r["ObjectID"]);
            RepositoryTypeName = r["RepositoryTypeName"].ToString();
            Username = r["Username"].ToString();
            FavouritedDate = Convert.ToDateTime(r["FavouritedDate"]);
        }

        public Favourite(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectToFavourite)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"TypeName", objectToFavourite.GetType().Name},
                {"ObjectID", objectToFavourite.ID},
                {"RepositoryTypeName",objectToFavourite.Repository.GetType().Name},
                {"Username", Environment.UserName},
                {"FavouritedDate", DateTime.Now},
            });
        }

        public bool IsFavourite(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
        {
            return IsFavourite(mapsDirectlyToDatabaseTable.ID, mapsDirectlyToDatabaseTable.GetType());
        }

        public bool IsFavourite(int id, Type type)
        {
            return ObjectID == id && TypeName.Equals(type.Name);
        }
    }
}
