using FAnsi.Discovery;
using MongoDB.Driver.Linq;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.Datasets;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Modules.Mutilators.PostLoad
{
    public class PureDatasetPostLoadUpdater : IMutilateDataTables
    {
        private readonly string _type = typeof(PureDatasetProvider).ToString();
        private PureDatasetProvider _provider;
        private PureDataset _pureDataset;
        private int _newRecordsCount;
        private PureDate _earliestDate;
        private PureDate _latestDate;


        [DemandsInitialization("The Dataset to update")]
        public Curation.Data.Datasets.Dataset Dataset { get; set; }


        [DemandsInitialization("Update the time period associated with the dataset", DefaultValue = true)]
        public bool UpdateTimePeriods { get; set; }

        [DemandsInitialization("Update the description of the dataset", DefaultValue = true)]
        public bool UpdateDescription { get; set; }

        [DemandsInitialization("The Column within the data to use for temporal updates")]
        public string DateColumn { get; set; }

        [DemandsInitialization(@"The Text added to the description to denote the update.
Some variables are available:
%d - Todays date
%c - The number of new records
%s - The earliest date in the new records
%l - The latest date in the new records
", DefaultValue = "Update %d: Added %c Records with dates between %s and %l.")]
        public string DescriptionUpdateText { get; set; }

        public PureDatasetPostLoadUpdater() { }

        public void Check(ICheckNotifier notifier)
        {
            if (Dataset is null) notifier.OnCheckPerformed(new CheckEventArgs("No Dataset was selected", CheckResult.Fail));
            if (Dataset is not null && Dataset.Type != _type) notifier.OnCheckPerformed(new CheckEventArgs("Dataset is not a Pure Dataset.", CheckResult.Fail));
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            if (loadStage != LoadStage.PostLoad)
            {
                throw new Exception("Dataset Updater can only be done in the PostLoad stage.");
            }
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
        }

        public ExitCodeType Mutilate(IDataLoadJob job)
        {
            var catalogues = job.LoadMetadata.GetAllCatalogues();
            if (catalogues.Count() != 1)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Can only update a pure dataset when the load affects a single catalogue."));
                return ExitCodeType.Error;
            }
            var catalogue = catalogues.First();
            var column = catalogue.CatalogueItems.Where(ci => ci.ColumnInfo.GetRuntimeName() == DateColumn).First();
            // get latest dataload id and use that to run a query
            var dataLoadID = job.JobID;

            var qb = new QueryBuilder(null, null);
            qb.AddColumn(new ColumnInfoToIColumn(new MemoryRepository(), column.ColumnInfo));
            qb.AddCustomLine($"{SpecialFieldNames.DataLoadRunID} = {dataLoadID}", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
            var sql = qb.SQL;

            var dt = new DataTable();
            dt.BeginLoadData();
            var server = catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad,false);
            var con = server.GetConnection();
            con.Open();
            using (var cmd = server.GetCommand(sql, con))
            {
                using var da = server.GetDataAdapter(cmd);
                da.Fill(dt);
            }
            dt.EndLoadData();
            if (dt.Rows.Count ==0) return ExitCodeType.OperationNotRequired;
            _newRecordsCount = dt.Rows.Count;
            _earliestDate = new PureDate(dt.AsEnumerable().Min(row => DateTime.Parse(row[0].ToString())));
            _latestDate = new PureDate(dt.AsEnumerable().Max(row => DateTime.Parse(row[0].ToString())));

            _provider = new PureDatasetProvider(new ThrowImmediatelyActivator(job.RepositoryLocator), job.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("ID", Dataset.Provider_ID).First());
            _pureDataset = _provider.FetchPureDataset(Dataset);
            var datasetUpdate = new PureDataset();
            if (UpdateDescription)
            {
                datasetUpdate.Descriptions = GetDescriptions();
            }
            if (UpdateTimePeriods)
            {
                datasetUpdate.TemporalCoveragePeriod = GetTemporalCoveragePeriod();
            }
            _provider.Update(_pureDataset.UUID, datasetUpdate);

            return ExitCodeType.Success;
        }

        private TemporalCoveragePeriod GetTemporalCoveragePeriod()
        {
            var coverage = _pureDataset.TemporalCoveragePeriod ?? new TemporalCoveragePeriod();
            if(_pureDataset.TemporalCoveragePeriod is null || _pureDataset.TemporalCoveragePeriod.StartDate is null || _earliestDate.IsBefore(_pureDataset.TemporalCoveragePeriod.StartDate))
                coverage.StartDate = _earliestDate;

            if(_pureDataset.TemporalCoveragePeriod is null ||  _pureDataset.TemporalCoveragePeriod.EndDate is null || _pureDataset.TemporalCoveragePeriod.EndDate.IsBefore(_latestDate))
                coverage.EndDate = _latestDate;
            return coverage;
        }

        private string GetUpdateText()
        {

            return DescriptionUpdateText.Replace("%d", DateTime.Now.ToString("dd/MM/yyyy")).Replace("%c", $"{_newRecordsCount}").Replace("%s", $"{_earliestDate.Day}/{_earliestDate.Month}/{_earliestDate.Year}").Replace("%l", $"{_latestDate.Day}/{_latestDate.Month}/{_latestDate.Year}");
        }

        private List<PureDescription> GetDescriptions()
        {
            var descriptions = new List<PureDescription>();
            var datasetDescriptionTerm = "/dk/atira/pure/dataset/descriptions/datasetdescription";
            foreach (var description in _pureDataset.Descriptions.Where(d => d.Term.URI == datasetDescriptionTerm))
            {
                description.Value.en_GB += @$"
{GetUpdateText()}";
                descriptions.Add(description);
            }

            return descriptions;
        }

    }
}
