using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DataTableExtension;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    [Description("This source generates a linked (anonymised) dataset using a linkage cohort in the same way as ExecuteDatasetExtractionSource with the exception that it copies the Cohort from the cohort database into tempdb (allowing cross server data extraction)")]
    public class ExecuteCrossServerDatasetExtractionSource : ExecuteDatasetExtractionSource
    {
        private bool _haveCopiedCohortAndAdjustedSql = false;
        private const string tempDBName = "tempdb";

        public override DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (!_haveCopiedCohortAndAdjustedSql)
                CopyCohortToDataServer(listener);

            return base.GetChunk(listener, cancellationToken);
        }
        
        public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        {
            //call base hacks
            sql = base.HackExtractionSQL(sql, listener);

            //now replace database with tempdb
            var sourceDatabaseName = SqlSyntaxHelper.GetRuntimeName(Request.ExtractableCohort.ExternalCohortTable.Database);


            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to replace references to database " + sourceDatabaseName + " with " + tempDBName, null));


             listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Original (unhacked) SQL was " + sql,null));

            //matches .dbo. and .. and .anythingelse. were the brackets make group[1] the database name and group[2] the schema name
            Regex RegexDatabaseName = new Regex(@"(\[?"+sourceDatabaseName+@"\]?)\.([0-9a-zA-Z$_ ]*)\.");
            
            foreach (Match match in RegexDatabaseName.Matches(sql))
            {
                if (match.Success)
                {
                    string dbName = match.Groups[1].Value;
                    string schemaName = match.Groups[2].Value;

                    sql = sql.Replace(dbName + "." + schemaName + ".", tempDBName + "..");
                }
            }


            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Adjusted (hacked) SQL was " + sql, null));

            //replace [MyCohortDatabase].. with [tempdb].. (while dealing with Cohort..Cohort replacement correctly as well as 'Cohort.dbo.Cohort.Fish' correctly)
            return sql;
        }

        private List<string> tablesIntempDbToNuke = new List<string>();
        
        public static Semaphore OneCrossServerExtractionAtATime = new Semaphore(1, 1);
        private DiscoveredServer _server;

        private void CopyCohortToDataServer(IDataLoadEventListener listener)
        {
            DataTable cohort = null;

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"About to wait for Semaphore OneCrossServerExtractionAtATime to become available"));
            OneCrossServerExtractionAtATime.WaitOne(-1);
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Captured Semaphore OneCrossServerExtractionAtATime"));

            try
            {
               cohort = Request.ExtractableCohort.FetchEntireCohort();
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while trying to download the cohort from the Cohort server (in preparation for transfering it to the data server for linkage and extraction)",e);
            }
            
            DataTableHelper  helper = new DataTableHelper(cohort);

            _server = Request.Catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, false);
            _server.ChangeDatabase(tempDBName);

            helper.CommitDataTableToTempDB(_server, true);
            
            tablesIntempDbToNuke.Add(cohort.TableName);
            //table will now be in tempdb

            _haveCopiedCohortAndAdjustedSql = true;
        }


        public override void Dispose(IDataLoadEventListener job, Exception pipelineFailureExceptionIfAny)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to release Semaphore OneCrossServerExtractionAtATime"));
            OneCrossServerExtractionAtATime.Release(1);
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Released Semaphore OneCrossServerExtractionAtATime"));

            if(!_server.GetCurrentDatabase().GetRuntimeName().Equals("tempdb"))
                throw new Exception("Somehow _tempDBInfo is no longer pointed at tempdb! good job we checked, we are about to start nuking tables");

            DbConnection dbConnection = _server.GetConnection();
            try
            {
                dbConnection.Open();

                foreach (string toNuke in tablesIntempDbToNuke)
                {
                    try
                    {
                        DbCommand cmd = _server.GetCommand("Drop Table " + toNuke, dbConnection);
                        cmd.ExecuteNonQuery();
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Dropped table from tempdb:" + toNuke));
                    }
                    catch (Exception e)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Could not drop table " + toNuke, e));
                    }
                }
            }
            catch (Exception e)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Could not connect to temp db to delete tables in it",e));
            }
            finally
            {
                dbConnection.Close();
            }
           
          
            base.Dispose(job, pipelineFailureExceptionIfAny);
        }

        public override void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Checking not supported for Cross Server extraction since it involves shipping off the cohort into tempdb.",CheckResult.Warning));
        }

        public override DataTable TryGetPreview()
        {
            throw new NotSupportedException("Previews are not supported for Cross Server extraction since it involves shipping off the cohort into tempdb.");
        }
    }
}
