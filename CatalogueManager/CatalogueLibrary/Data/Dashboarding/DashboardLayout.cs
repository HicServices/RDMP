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
    /// Describes a named collection of windows helpful for achieving a given task (usually data summarisation).  This class is the root object and has name (e.g. Dave's Dashboard).  It then
    /// has a collection of DashboardControls which are IDashboardableControl instances that the user has configured on his Dashboard via DashboardLayoutUI.  This can include plugins. Not only
    /// does this class provide persistence for useful layouts of controls between application executions but it allows users to share their dashboards with one another.
    /// </summary>
    public class DashboardLayout : DatabaseEntity,INamed
    {
        #region Database Properties
        private string _name;
        private DateTime _created;
        private string _username;

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        /// <summary>
        /// The time the new dashboard was created
        /// </summary>
        public DateTime Created
        {
            get { return _created; }
            set { SetField(ref _created, value); }
        }

        /// <summary>
        /// The user who created the dashboard
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value); }
        }
        #endregion

        #region Relationships

        /// <summary>
        /// Returns all controls that should be rendered on the given dashboard
        /// </summary>
        [NoMappingToDatabase]
        public DashboardControl[] Controls{ get { return Repository.GetAllObjectsWithParent<DashboardControl>(this); }}
        #endregion

        internal DashboardLayout(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"].ToString();
            Created = Convert.ToDateTime(r["Created"]);
            Username = r["Username"].ToString();
        }

        /// <summary>
        /// Creates a new empty dashboard with the given name ready for controls to be added by the user
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name"></param>
        public DashboardLayout(ICatalogueRepository repository, string name)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Username",Environment.UserName},
                {"Name",name}
            });   
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
