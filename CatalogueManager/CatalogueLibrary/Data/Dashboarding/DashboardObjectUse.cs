using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Referencing;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// Describes a specific object used by a DashboardControl.  For example if you create a pie chart of issues on a specific catalogue on your DashboardLayout then there will be a 
    /// DashboardControl for the pie chart and a DashboardObjectUse pointing at that specific Catalogue.  These refernces do not stop objects being deleted.  References can also be 
    /// cross database (e.g. pointing at objects in a DataExport database like Project etc).
    /// </summary>
    public class DashboardObjectUse: ReferenceOtherObjectDatabaseEntity
    {
        #region Database Properties

        private int _dashboardControlID;

        /// <summary>
        /// The <see cref="DashboardControl"/> for which the class records object usage for
        /// </summary>
        public int DashboardControl_ID
        {
            get { return _dashboardControlID; }
            set { SetField(ref _dashboardControlID , value); }
        }
        
        

        #endregion

        internal DashboardObjectUse(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            DashboardControl_ID = Convert.ToInt32(r["DashboardControl_ID"]);

        }
         
        /// <summary>
        /// Records the fact that the given <see cref="DashboardControl"/> targets the given object (and hopefully displays information about it)
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parentControl"></param>
        /// <param name="objectToSave"></param>
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
