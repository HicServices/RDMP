using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging.PastEvents
{
    /// <summary>
    /// Used when requesting a specific recent ArchivalDataLoadInfo for a given task.
    /// </summary>
    public enum PastEventType
    {
        MostRecent,
        LastsuccessfulLoad
    }


    /// <summary>
    /// Readonly historical version of DataLoadInfo.  The central hierarchical RDMP logging database records activites across all areas of the program in a central
    /// place.  You can process these records programatically via LogManager.  This class contains public properties for each of the sub concepts (Errors, Progress
    /// messages, Tables loaded etc).  See Logging.cd for more information
    /// </summary>
    public class ArchivalDataLoadInfo : IArchivalLoggingRecordOfPastEvent, IComparable
    {
        public int ID { get; private set; }
        public int DataLoadTaskID { get; set; }
        public const int MaxDescriptionLength = 300;

        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }

        public const int MaxChildrenToFetch = 1000;

        public bool HasUnresolvedErrors {
            get
            {
                return Errors.Any(e => e.Explanation == null);
            }
        }
        public string ToShortString()
        {
            var s = ToString();
            if (s.Length > ArchivalDataLoadInfo.MaxDescriptionLength)
                return s.Substring(0, ArchivalDataLoadInfo.MaxDescriptionLength) + "...";
            return s;
        }
        public override string ToString()
        {
            string elapsed = "";
            if (EndTime != null)
            {
                var ts = EndTime.Value.Subtract(StartTime);
                elapsed = " (" + ts.ToString(@"hh\:mm\:ss")+ ")";
            }

            return Description + "(ID="+ID +") - " + StartTime + " - " + (EndTime != null ? EndTime.ToString() : "<DidNotFinish>") + elapsed;
        }

    
        /// <summary>
        /// All tables loaded during the run (up to <see cref="MaxChildrenToFetch"/>)
        /// </summary>
        public List<ArchivalTableLoadInfo>  TableLoadInfos = new List<ArchivalTableLoadInfo>();

        /// <summary>
        /// All errors that occured during the run  (up to <see cref="MaxChildrenToFetch"/>)
        /// </summary>
        public List<ArchivalFatalError> Errors = new List<ArchivalFatalError>();

        /// <summary>
        /// All progress messages recorded during the run (up to <see cref="MaxChildrenToFetch"/>)
        /// </summary>
        public List<ArchivalProgressLog> Progress = new List<ArchivalProgressLog>();

        public string Description { get; set; }

        private ArchivalDataLoadInfo(int id, int dataLoadTaskID)
        {
            ID = id;
            DataLoadTaskID = dataLoadTaskID;
        }
        
        public static ArchivalDataLoadInfo GetLoadStatusOf(PastEventType type, string dataTask, DiscoveredServer lds)
        {
            if(type == PastEventType.MostRecent)
                return GetLoadHistoryForTask(dataTask, lds, top1: true).FirstOrDefault();
            else if (type == PastEventType.LastsuccessfulLoad)
                return
                    GetLoadHistoryForTask(dataTask,lds).Where(lh => lh.HasUnresolvedErrors == false)
                        .OrderByDescending(lh => lh.StartTime)
                        .FirstOrDefault();
            else
            {
                throw new NotSupportedException("did not recognise type " + type);
            }
        }

        /// <summary>
        /// Returns up to <see cref="MaxChildrenToFetch"/> data load audit objects which describe runs of over arching task <paramref name="dataTask"/>
        /// </summary>
        /// <param name="dataTask"></param>
        /// <param name="lds"></param>
        /// <param name="top1"></param>
        /// <param name="token"></param>
        /// <param name="specificDataLoadRunIDOnly"></param>
        /// <returns></returns>
        public static IEnumerable<ArchivalDataLoadInfo> GetLoadHistoryForTask(string dataTask, DiscoveredServer lds, bool top1 = false, CancellationToken? token = null, int? specificDataLoadRunIDOnly = null)
        {
            DataTable runsIncludingErrorData;
            DataTable progressData;
            DataTable tableAndDataSources;


            if (!GetTableData(out runsIncludingErrorData, out progressData, out tableAndDataSources, lds,dataTask, top1, token,specificDataLoadRunIDOnly))
                yield break;

            
            int currentRunID = -1;

            ArchivalDataLoadInfo currentArchivalDataLoadInfo = null;
            int progressTableIdx = 0;
            int tableInfoIdx = 0;

            //foreach error/dataload
            foreach (DataRow row in runsIncludingErrorData.Rows)
            {

                //it is a new data load run we haven't seen before
                if ((int) row["runID"] != currentRunID)
                {
                    if(currentArchivalDataLoadInfo != null)
                        yield return currentArchivalDataLoadInfo;//we have found a new one so yield the last one

                    //and start a record for the new one
                    currentArchivalDataLoadInfo = new ArchivalDataLoadInfo(Convert.ToInt32(row["runID"]), Convert.ToInt32(row["dataLoadTaskID"]));

                    //populate basic facts from the table
                    currentArchivalDataLoadInfo.StartTime = (DateTime)row["startTime"]  ;
                    if (row["endTime"] == null || row["endTime"] == DBNull.Value)
                        currentArchivalDataLoadInfo.EndTime = null;
                    else
                        currentArchivalDataLoadInfo.EndTime = Convert.ToDateTime(row["endTime"]);

                    currentArchivalDataLoadInfo.Description = row["runDescription"] as string;
                    currentRunID = (int) row["runID"];


                    //now go and lookup the progress data table to see what progress is recorded for it( this works because both tables are sorted on index so we just for through it and break if we find one that does not match)
                    //iterate the progress data
                    for (; progressTableIdx < progressData.Rows.Count; progressTableIdx++)
                    {
                        //if the progress data belongs to this run
                        if ((int) progressData.Rows[progressTableIdx]["runID"] == currentRunID)
                        {
                            currentArchivalDataLoadInfo.Progress.Add(
                                new ArchivalProgressLog(
                                    (int) progressData.Rows[progressTableIdx]["ID"],
                                    (DateTime) progressData.Rows[progressTableIdx]["time"],
                                    progressData.Rows[progressTableIdx]["eventType"].ToString(),
                                    progressData.Rows[progressTableIdx]["description"].ToString()));
                        }
                        else
                        {
                            break; //we have stopped looking at progress about this load so break out
                        }
                    }


                    ArchivalTableLoadInfo lastTableLoadInfo=null;

                    //now go and lookup the table infos table to see what table loads occurred during it( this works because both tables are sorted on index so we just for through it and break if we find one that does not match)
                    //iterate the progress data
                    for (; tableInfoIdx < tableAndDataSources.Rows.Count; tableInfoIdx++)
                    {
                        //if the progress data belongs to this run
                        if ((int)tableAndDataSources.Rows[tableInfoIdx]["runID"] == currentRunID)
                        {
                            object tlid = tableAndDataSources.Rows[tableInfoIdx]["tableLoadRunID"];

                            if (tlid == DBNull.Value)
                                continue;

                            int currentTableLoadRunID = Convert.ToInt32(tlid);

                            //its a new one
                            if (lastTableLoadInfo == null || currentTableLoadRunID != lastTableLoadInfo.ID)
                            {
                                
                                lastTableLoadInfo = new ArchivalTableLoadInfo(
                                currentArchivalDataLoadInfo,
                                currentTableLoadRunID,
                                (DateTime)tableAndDataSources.Rows[tableInfoIdx]["startTime"],
                                tableAndDataSources.Rows[tableInfoIdx]["endTime"],
                                tableAndDataSources.Rows[tableInfoIdx]["targetTable"] as string,
                                tableAndDataSources.Rows[tableInfoIdx]["inserts"],
                                tableAndDataSources.Rows[tableInfoIdx]["updates"],
                                tableAndDataSources.Rows[tableInfoIdx]["deletes"],
                                tableAndDataSources.Rows[tableInfoIdx]["notes"] as string);

                                currentArchivalDataLoadInfo.TableLoadInfos.Add(lastTableLoadInfo);
                            }

                            //add a new datasource regadless
                            lastTableLoadInfo.DataSources.Add(new ArchivalDataSource(
                                (int)tableAndDataSources.Rows[tableInfoIdx]["DataSourceID"],
                                tableAndDataSources.Rows[tableInfoIdx]["originDate"],
                                tableAndDataSources.Rows[tableInfoIdx]["source"] as string,
                                tableAndDataSources.Rows[tableInfoIdx]["archive"] as string,
                                tableAndDataSources.Rows[tableInfoIdx]["MD5"] as string));
                        }
                        else
                        {
                            break; //we have stopped looking at progress about this load so break out
                        }
                    }
                }


                //create a new error because this denormalised row has error data on it
                if (row["fatalID"] != null && row["fatalID"] != DBNull.Value)
                {
                    currentArchivalDataLoadInfo.Errors.Add(new ArchivalFatalError(
                        Convert.ToInt32(row["fatalID"]),
                        Convert.ToDateTime(row["fatalTime"]),
                        row["source"] as string,
                        row["errorDescription"] as string,
                        row["explanation"] as string
                        ));      
                }
            }

            //yield the final row too
            if (currentArchivalDataLoadInfo != null)
                yield return currentArchivalDataLoadInfo;//we have found a new one so yield the last one

               
        }

        private static bool GetTableData(out DataTable runsIncludingErrorData, out DataTable progressData, out DataTable tableAndDataSources, DiscoveredServer lds, string dataTask = null, bool top1 = false, CancellationToken? token = null, int? specificDataLoadRunIDOnly = null)
        {
            if (specificDataLoadRunIDOnly != null && top1)
                throw new NotSupportedException("Cannot have TOP 1 (load) and specificDataLoadRunIDOnly both set");
            
            runsIncludingErrorData = new DataTable();
            progressData = new DataTable();
            tableAndDataSources = new DataTable();

            if (token!= null)
                lds.EnableAsync();

            using(var con = (SqlConnection)lds.GetConnection())
            {
                var databaseName = lds.GetCurrentDatabase();

                if (token != null)
                    con.OpenAsync(token.Value).Wait(5000,token.Value);
                else
                    con.Open();

                string whereText = "";
                List<string> whereBits = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(dataTask))
                    whereBits.Add("task.name= @task");

                if (top1)
                    whereBits.Add("run.ID in (SELECT MAX(ID) from [" + databaseName + @"].[dbo].[DataLoadRun] where dataLoadTaskID = task.ID) ");

                if (specificDataLoadRunIDOnly != null)
                    whereBits.Add("run.ID = " + specificDataLoadRunIDOnly.Value);

                whereText = string.Join(" AND ", whereBits);
                
                #region get data about errors
                SqlCommand cmdErrors = new SqlCommand(
                    string.Format(@"
SELECT  TOP {1}
run.dataLoadTaskID dataLoadTaskID,
run.description runDescription,
	run.ID runID
	,startTime
      ,[endTime]
       ,fatal.ID fatalID
       ,fatal.time fatalTime
      ,fatal.source
	  ,fatal.description errorDescription
	  ,fatal.explanation
  FROM [" + databaseName + @"].[dbo].[DataLoadRun] run
  left join 
  [" + databaseName + @"].[dbo].FatalError as fatal
  on  fatal.[dataLoadRunID]=run.ID
 left join
  [" + databaseName + @"].[dbo].DataLoadTask task
  on
  run.dataLoadTaskID = task.ID
  where 
{0}
  order by run.ID desc
", whereText, MaxChildrenToFetch)
                    , con);
                
                if (!string.IsNullOrWhiteSpace(dataTask))
                    cmdErrors.Parameters.Add("@task", SqlDbType.VarChar, 100).Value = dataTask;

                SqlDataAdapter da = new SqlDataAdapter(cmdErrors);
                da.Fill(runsIncludingErrorData);
                #endregion

                #region get data about progress
                SqlCommand cmdProgress = new SqlCommand(
                   string.Format(@"
SELECT TOP {1}
run.ID runID,
progress.*
  FROM [" + databaseName + @"].[dbo].ProgressLog progress
  join 
  [" + databaseName + @"].[dbo].DataLoadRun run
  on 
  progress.dataLoadRunID = run.ID 
left join
  [" + databaseName + @"].[dbo].DataLoadTask task
  on
  run.dataLoadTaskID = task.ID
  where
{0}
  order by run.ID desc", whereText, MaxChildrenToFetch*10)
                    , con);

                if (!string.IsNullOrWhiteSpace(dataTask))
                    cmdProgress.Parameters.Add("@task", SqlDbType.VarChar, 100).Value = dataTask;

                da = new SqlDataAdapter(cmdProgress);
                da.Fill(progressData);
                
                #endregion

                #region get data about table load infos including datasource
                SqlCommand cmdTableLoadInfo = new SqlCommand(
 string.Format(@"
SELECT  TOP {1}
run.ID runID,
t.*,
s.ID DataSourceID,
s.*
  FROM [" + databaseName + @"].[dbo].TableLoadRun t
  join 
  [" + databaseName + @"].[dbo].DataLoadRun run
  on 
  t.dataLoadRunID = run.ID
  left join 
  [" + databaseName + @"].[dbo].DataSource s
  on
  t.ID = s.tableLoadRunID
 left join
  [" + databaseName + @"].[dbo].DataLoadTask task
  on
  run.dataLoadTaskID = task.ID
  where 
{0}
  order by run.ID desc
", whereText, MaxChildrenToFetch)
                    , con);
                
                if(!string.IsNullOrWhiteSpace(dataTask))
                    cmdTableLoadInfo.Parameters.Add("@task", SqlDbType.VarChar, 100).Value = dataTask;

                da = new SqlDataAdapter(cmdTableLoadInfo);
                da.Fill(tableAndDataSources);


                #endregion

                con.Close();
            }
            return true;
        }

        public int CompareTo(object obj)
        {
            var other = obj as ArchivalDataLoadInfo;
            if (other != null)
                if (StartTime == other.StartTime)
                    return 0;
                else
                    return StartTime > other.StartTime ? 1 : -1;

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }
    }
}