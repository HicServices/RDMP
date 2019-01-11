
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode.DataAccess;

namespace CohortManagerLibrary.Execution.Joinables
{
    /// <summary>
    /// A single AggregateConfiguration being executed by a CohortCompiler which is defined as a JoinableCohortAggregateConfiguration.  The 
    /// AggregateConfiguration will be a query like 'select distinct patientId, drugName,prescribedDate from  TableX where ...'.  The  query
    /// result table can/will be commited as a CacheCommitJoinableInceptionQuery to  the CachedAggregateConfigurationResultsManager.
    /// </summary>
    public class JoinableTask:CacheableTask
    {
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;
        private AggregateConfiguration _aggregate;
        private string _catalogueName;

        public JoinableCohortAggregateConfiguration Joinable { get; private set; }

        
        public JoinableTask(JoinableCohortAggregateConfiguration joinable, CohortCompiler compiler) : base(compiler)
        {
            
            Joinable = joinable;
            _aggregate = Joinable.AggregateConfiguration;
            _cohortIdentificationConfiguration =_aggregate.GetCohortIdentificationConfigurationIfAny();
            
            _catalogueName = Joinable.AggregateConfiguration.Catalogue.Name;
            RefreshIsUsedState();
        }

        public override string GetCatalogueName()
        {
            return _catalogueName;
        }

        public override IMapsDirectlyToDatabaseTable Child
        {
            get { return Joinable; }
        }

        public override IDataAccessPoint[] GetDataAccessPoints()
        {
            return Joinable.AggregateConfiguration.Catalogue.GetTableInfoList(false);
        }

        public override string ToString()
        {
            
            string name = _aggregate.Name;

            string expectedTrimStart = _cohortIdentificationConfiguration.GetNamingConventionPrefixForConfigurations();

            if (name.StartsWith(expectedTrimStart))
                return name.Substring(expectedTrimStart.Length);

            return name;
        }
        
        public override AggregateConfiguration GetAggregateConfiguration()
        {
            return Joinable.AggregateConfiguration;
        }

        public override CacheCommitArguments GetCacheArguments(string sql, DataTable results, DatabaseColumnRequest[] explicitTypes)
        {
            return new CacheCommitJoinableInceptionQuery(Joinable.AggregateConfiguration, sql, results, explicitTypes, Timeout);
        }

        public override void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager)
        {
            manager.DeleteCacheEntryIfAny(Joinable.AggregateConfiguration, AggregateOperation.JoinableInceptionQuery);
        }

        public override int Order { get { return Joinable.ID; }
            set { throw new NotSupportedException();}
        }

        public bool IsUnused { get; private set; }

        public void RefreshIsUsedState()
        {
            IsUnused = !Joinable.Users.Any();
        }

        public string GetUnusedWarningText()
        {
            return
"Patient Index Table '" + ToString() + @"' is not used by any of your sets (above).";
        }
    }
}
