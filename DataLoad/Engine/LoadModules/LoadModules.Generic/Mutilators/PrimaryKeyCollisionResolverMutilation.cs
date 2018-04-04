using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.QueryBuilding;
using DataLoadEngine.Job;
using DataLoadEngine.Mutilators;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// Data load component which can delete records in an (unconstrained) RAW table to enforce uniqueness of the primary key field (as it is configured in LIVE).
    /// This lets you resolve non-exact duplication based on column order (e.g. if there is a collision where one has an later 'DataAge' field then use the later
    /// one and discard the earlier one.
    /// 
    /// <para>This component requires that a collision resolution order has been configured on the TableInfo (See ConfigurePrimaryKeyCollisionResolution)</para>
    /// </summary>
    [Description("This is a very dangerous operation which uses the primary key collision resolution order (Accessible through CatalogueManager by right clicking a TableInfo and choosing 'Configure Primary Key Collision Resolution') to delete records in a preferred order, fully eliminating primary key collisions.  It is a very good idea to not have this task until you are absolutely certain that your primary key is correct and that the duplicate records being deleted are the correct decisions e.g. delete an older record in a given load batch and not simply erasing vast swathes of data!.  The Data Load Engine will tell you with a warning when records are deleted and how many.  If you notice a lot of deletion then try removing this component and manually inspecting the data in the RAW database after the data load fails (due to unresolved primary key conflicts)")]
    public class PrimaryKeyCollisionResolverMutilation : IPluginMutilateDataTables
    {

        [DemandsInitialization("The table on which to resolve primary key collisions, must have PrimaryKeyCollision resolution setup for it in the Data Catalogue", Mandatory = true)]
        public TableInfo TargetTable { get; set; }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {

        }
   
        private DiscoveredDatabase _dbInfo;
        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            if (loadStage != LoadStage.AdjustRaw)
                throw new Exception("Primary key collisions can only be resolved in a RAW environment, current load stage is:" + loadStage + " (The reason for this is because there should be primary keys in the database level in STAGING and LIVE making primary key collisions IMPOSSIBE)");

            _dbInfo = dbInfo;
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            ResolvePrimaryKeyConflicts(job);
            return ExitCodeType.Success;
        }


        private void ResolvePrimaryKeyConflicts(IDataLoadEventListener job)
        {

            SqlConnection con = (SqlConnection) _dbInfo.Server.GetConnection();
            con.Open();

            PrimaryKeyCollisionResolver resolver = new PrimaryKeyCollisionResolver(TargetTable);
            SqlCommand cmdAreTherePrimaryKeyCollisions = new SqlCommand(resolver.GenerateCollisionDetectionSQL(), con);
            cmdAreTherePrimaryKeyCollisions.CommandTimeout = 5000;

            //if there are no primary key collisions
            if (cmdAreTherePrimaryKeyCollisions.ExecuteScalar().ToString().Equals("0"))
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No primary key collisions detected"));
                return;
            }

            //there are primary key collisions so resolve them
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Primary key collisions detected"));

            SqlCommand cmdResolve = new SqlCommand(resolver.GenerateSQL(), con);
            cmdResolve.CommandTimeout = 5000;
            int affectedRows = cmdResolve.ExecuteNonQuery();

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Primary key collisions resolved by deleting " + affectedRows + " rows"));
            con.Close();

        }

        public void Check(ICheckNotifier notifier)
        {
     
            if (TargetTable == null)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Target table is null, a table must be specified upon which to resolve primary key duplication (that TableInfo must have a primary key collision resolution order)",
                    CheckResult.Fail, null));

            try
            {
                PrimaryKeyCollisionResolver resolver = new PrimaryKeyCollisionResolver(TargetTable);
                string sql = resolver.GenerateSQL();
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to check PrimaryKeyCollisionResolver on " + TargetTable,CheckResult.Fail, e));
            }
            
        }
    }
}
