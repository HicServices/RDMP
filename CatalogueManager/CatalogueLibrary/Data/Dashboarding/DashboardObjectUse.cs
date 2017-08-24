using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// Describes a specific object used by a DashboardControl.  For example if you create a pie chart of issues on a specific catalogue on your DashboardLayout then there will be a 
    /// DashboardControl for the pie chart and a DashboardObjectUse pointing at that specific Catalogue.  These refernces do not stop objects being deleted.  References can also be 
    /// cross database (e.g. pointing at objects in a DataExport database like Project etc).
    /// </summary>
    public class DashboardObjectUse:DatabaseEntity
    {
        #region Database Properties

        private string _typeName;
        private int _objectID;
        private string _repositoryTypeName;
        private int _dashboardControlID;

        public int DashboardControl_ID
        {
            get { return _dashboardControlID; }
            set { SetField(ref _dashboardControlID , value); }
        }

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


        #endregion

        public DashboardObjectUse(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            DashboardControl_ID = Convert.ToInt32(r["DashboardControl_ID"]);
            TypeName = r["TypeName"].ToString();
            ObjectID = Convert.ToInt32(r["ObjectID"]);
            RepositoryTypeName = r["RepositoryTypeName"].ToString();

        }

        public DashboardObjectUse(ICatalogueRepository repository, DashboardControl parentControl, IMapsDirectlyToDatabaseTable objectToSave)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"DashboardControl_ID",parentControl.ID},
                {"TypeName", objectToSave.GetType().Name},
                {"ObjectID", objectToSave.ID},
                {"RepositoryTypeName",objectToSave.Repository.GetType().Name}
            });
        }
    }
}
