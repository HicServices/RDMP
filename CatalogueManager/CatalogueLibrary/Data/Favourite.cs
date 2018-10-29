using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Referencing;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Allows you to store a record of a faviourite database object including all objects in Catalogue and DataExport databases.  The Objects type and ID are stored and then 
    /// whenever an RDMPCollectionUI is visible and that object is onscreen a star will appear beside it.  Favourites are stored on a 'per user' basis in the Catalogue database so 
    /// even if you switch computers/change sessions Favourites are preserved. 
    /// </summary>
    public class Favourite:ReferenceOtherObjectDatabaseEntity
    {
        #region Database Properties
        private string _username;
        private DateTime _favouritedDate;
        
        /// <summary>
        /// The user that favourited the object
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { SetField(ref _username , value); }
        }

        /// <summary>
        /// When the <see cref="Username"/> favourited the object
        /// </summary>
        public DateTime FavouritedDate
        {
            get { return _favouritedDate; }
            set { SetField(ref _favouritedDate, value); }
        }
        #endregion

        internal Favourite(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            Username = r["Username"].ToString();
            FavouritedDate = Convert.ToDateTime(r["FavouritedDate"]);
        }


        /// <summary>
        /// Records that the current Environment.UserName wants to mark the <paramref name="objectToFavourite"/> as one of his favourite objects
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="objectToFavourite"></param>
        public Favourite(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectToFavourite)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ReferencedObjectID",objectToFavourite.ID},
                {"ReferencedObjectType",objectToFavourite.GetType().Name},
                {"ReferencedObjectRepositoryType",objectToFavourite.Repository.GetType().Name},
                {"Username", Environment.UserName},
                {"FavouritedDate", DateTime.Now},
            });
        }
    }
}
