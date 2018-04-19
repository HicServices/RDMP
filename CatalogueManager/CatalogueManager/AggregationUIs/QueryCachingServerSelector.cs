using System;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;

namespace CatalogueManager.AggregationUIs
{
    /// <summary>
    /// Query Caching is the process by which the results of your cohort identification queries are indexed and stored in a purpose created database for performance.  By creating a 
    /// Query Caching database you can execute your cohort identification aggregates and store the resulting patient identifier list into the cache.
    /// 
    /// <para>This is vital for performance once your cohort identification criteria goes beyond 4 or 5 datasets (assuming non trivial dataset size) or when your dataset queries take a long
    /// time to run.  The CohortManager will use cached data where available to speed up execution.  </para>
    /// 
    /// <para>IMPORTANT: Make sure to put proper access controls on the Query Caching database just as if it was any of your repository datasets since identifier lists are considered sensitive
    /// information and should not be disclosed to any system user who could not otherwise access the datasets on which the queries themselves run anyway.</para>
    /// </summary>
    public partial class QueryCachingServerSelector : RDMPUserControl
    {
        public QueryCachingServerSelector()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (VisualStudioDesignMode)
                return;
            
            RefreshUIFromDatabase();
        }

        public event Action SelectedServerChanged;

        public ExternalDatabaseServer SelecteExternalDatabaseServer
        {
            get { return ddSelectQueryCachingDatabase.SelectedItem as ExternalDatabaseServer; ; }
            set
            {
                refreshing = true;
                if(value == null)
                    ddSelectQueryCachingDatabase.SelectedItem = null;
                else
                {
                    var toSelect = ddSelectQueryCachingDatabase.Items.Cast<ExternalDatabaseServer>().SingleOrDefault(s => s.ID == value.ID);
                    ddSelectQueryCachingDatabase.SelectedItem = toSelect;

                    if(toSelect == null)
                        throw new Exception("you just asked to select an ExternalDatabaseServer that isn't part of this drop downs Item collection! - how did that happen");
                }

                refreshing = false;
            }
        }

        private void btnCreateNewQueryCachingDatabase_Click(object sender, EventArgs e)
        {
            var dbAssembly = typeof (QueryCaching.Database.Class1).Assembly;
            CreatePlatformDatabase createPlatform = new CreatePlatformDatabase(dbAssembly);
            createPlatform.ShowDialog(this);

            if (!string.IsNullOrWhiteSpace(createPlatform.DatabaseConnectionString))
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(createPlatform.DatabaseConnectionString);

                var newServer = new ExternalDatabaseServer(RepositoryLocator.CatalogueRepository, "Caching Database", dbAssembly);

                newServer.Server = builder.DataSource;
                newServer.Database = builder.InitialCatalog;

                //if there is a username/password
                if (!builder.IntegratedSecurity)
                {
                    newServer.Password = builder.Password;
                    newServer.Username = builder.UserID;
                }
                newServer.SaveToDatabase();
                
                RefreshUIFromDatabase();
            }
        }

        private bool refreshing = false;
        public void RefreshUIFromDatabase()
        {
            if (RepositoryLocator == null)
                return;

            refreshing = true;
            ddSelectQueryCachingDatabase.Items.Clear();
            ddSelectQueryCachingDatabase.Items.AddRange(RepositoryLocator.CatalogueRepository.GetAllTier2Databases(Tier2DatabaseType.QueryCaching));
            refreshing = false;
        }

        private void ddSelectQueryCachingDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!refreshing)
                SelectedServerChanged();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ddSelectQueryCachingDatabase.SelectedItem = null;
        }

    }
}
