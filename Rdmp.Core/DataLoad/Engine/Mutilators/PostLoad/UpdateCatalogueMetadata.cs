using FAnsi.Discovery;
using MongoDB.Driver;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Engine.Mutilators.PostLoad
{
    class UpdateCatalogueMetadata : IMutilateDataTables
    {

        [DemandsInitialization("Update the time period associated with the dataset", DefaultValue = true)]
        public bool UpdateTimePeriods { get; set; }

        [DemandsInitialization("Update the description of the dataset", DefaultValue = true)]
        public bool UpdateDescription { get; set; }

        [DemandsInitialization("The Column within the data to use for temporal updates")]
        public string DateColumn { get; set; }


        [DemandsInitialization("Catalogues in Data Load to not update")]
        public List<Catalogue> CataloguestoIgnore { get; set; }

        [DemandsInitialization(@"The Text added to the description to denote the update.
Some variables are available:
%d - Todays date
%c - The number of new records
%s - The earliest date in the new records
%l - The latest date in the new records
", DefaultValue = "Update %d: Added %c Records with dates between %s and %l.")]
        public string DescriptionUpdateText { get; set; }

        public void Check(ICheckNotifier notifier)
        {
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
        }

        public ExitCodeType Mutilate(IDataLoadJob job)
        {
            if (!UpdateTimePeriods && !UpdateDescription) return ExitCodeType.OperationNotRequired;
            var catalogues = job.LoadMetadata.GetAllCatalogues().Where(c => !CataloguestoIgnore.Contains(c));
            foreach(var catalogue in catalogues)
            {
                var column = catalogue.CatalogueItems.Where(ci => ci.ColumnInfo.GetRuntimeName() == DateColumn).First();
                var dataLoadID = job.JobID;
                var qb = new QueryBuilder(null, null);
                qb.AddColumn(new ColumnInfoToIColumn(new MemoryRepository(), column.ColumnInfo));
                qb.AddCustomLine($"{SpecialFieldNames.DataLoadRunID} = {dataLoadID}", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
                var sql = qb.SQL;

                var dt = new DataTable();
                dt.BeginLoadData();
                var server = catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad, false);
                var con = server.GetConnection();
                con.Open();
                using (var cmd = server.GetCommand(sql, con))
                {
                    using var da = server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                dt.EndLoadData();
                if (dt.Rows.Count == 0) return ExitCodeType.OperationNotRequired;
                var latestDate = dt.AsEnumerable().Max(row => DateTime.Parse(row[0].ToString()));
                var earliestDate = dt.AsEnumerable().Min(row => DateTime.Parse(row[0].ToString()));
                var newRecordsCount = dt.Rows.Count;
                if (UpdateTimePeriods)
                {
                    catalogue.StartDate = earliestDate;
                    catalogue.EndDate = latestDate;
                }
                if (UpdateDescription)
                {
                    var description = catalogue.Description;
                    description = description + "\n\r" + DescriptionUpdateText.Replace("%d", DateTime.Now.ToString("dd/MM/yyyy")).Replace("%c", $"{newRecordsCount}").Replace("%s", $"{earliestDate.Day}/{earliestDate.Month}/{earliestDate.Year}").Replace("%l", $"{latestDate.Day}/{latestDate.Month}/{latestDate.Year}");
                }
                catalogue.SaveToDatabase();
            }

            return ExitCodeType.Success;
        }
    }
}
