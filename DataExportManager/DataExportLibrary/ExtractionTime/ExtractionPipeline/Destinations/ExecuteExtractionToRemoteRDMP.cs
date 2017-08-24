using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using HIC.Logging;
using LoadModules.Generic.DataProvider;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using RestSharp;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations
{
    public class ExecuteExtractionToRemoteRDMP : IExecuteDatasetExtractionDestination
    {
        private IExtractCommand extractCommand;
        private DataLoadInfo dataLoadInfo;
        private string destinationDescription;

        [DemandsInitialization("Remote WebAPI to create the extraction into. Data will be pushed to this server for merging with the remote RDMP Database",Mandatory = true)]
        public WebServiceConfiguration RemoteWebServer { get; set; }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, JsonConvert.SerializeObject(toProcess, Formatting.Indented)));

            try
            {
                extractCommand.State = ExtractCommandState.WaitingToExecute;
                if (TableLoadInfo == null)
                {
                    TableLoadInfo = new TableLoadInfo(dataLoadInfo,
                        String.Format("Sent rows to remote server {0}", RemoteWebServer.Endpoint),
                        RemoteWebServer.Endpoint + "?dataset=" + extractCommand.Name,
                        new[] { new DataSource(extractCommand.DescribeExtractionImplementation(), DateTime.UtcNow) },
                        toProcess.Rows.Count);
                }
                extractCommand.State = ExtractCommandState.WaitingForSQLServer;

                //Handle cancellation before pinging the server!
                if (cancellationToken.IsCancellationRequested || cancellationToken.IsAbortRequested || cancellationToken.IsStopRequested)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Aborted by user..."));
                    extractCommand.State = ExtractCommandState.UserAborted;
                    return null;
                }
                
                var handler = new HttpClientHandler { Credentials = new NetworkCredential(RemoteWebServer.Username, RemoteWebServer.GetDecryptedPassword()) };
                var client = new HttpClient(handler);
                var response = client.PostAsync(new Uri(RemoteWebServer.Endpoint + "?dataset=" + extractCommand.Name), 
                                                new StringContent(JsonConvert.SerializeObject(toProcess), new UTF8Encoding(), "application/json"),
                                                cancellationToken.StopToken).Result;
                var rowsInserted = 0;
                if (int.TryParse(response.Content.ReadAsStringAsync().Result, out rowsInserted))
                {
                    if (rowsInserted != toProcess.Rows.Count)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, 
                            String.Format("Unexpected number of rows returned from server. Extract has {0} rows, server accepted {1} rows ", toProcess.Rows.Count, rowsInserted)));
                        extractCommand.State = ExtractCommandState.Warning;
                    }
                    else
                    {
                        TableLoadInfo.Inserts += rowsInserted;
                        extractCommand.State = ExtractCommandState.Completed;
                    }
                }
                else
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Unexpected response from server: " + response));
                    extractCommand.State = ExtractCommandState.Crashed;
                }
            }
            catch (WebException e)
            {
                var message = "Failed sending data to remote";
                var response = e.Response as HttpWebResponse;
                if (response != null)
                {
                    message += ": " + response.StatusCode + " - " + response.StatusDescription;
                }
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, message, e));
                extractCommand.State = ExtractCommandState.Crashed;
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed sending data to remote", e));
                extractCommand.State = ExtractCommandState.Crashed;
            }
            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Job disposed..."));
            if (!TableLoadInfo.IsClosed)
                TableLoadInfo.CloseAndArchive();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Job aborted"));
        }

        public void Check(ICheckNotifier notifier)
        {
            if (RemoteWebServer == null || !RemoteWebServer.IsValid())
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Remote server has not been properly set (This component does not know where to extract data to!), to fix this you must edit the pipeline and choose a remote server to extract to)",
                        CheckResult.Fail));
                return;
            }
            try
            {
                var httpClient = new Http();
                httpClient.Url = new Uri(RemoteWebServer.Endpoint);
                httpClient.Credentials = new NetworkCredential(RemoteWebServer.Username, RemoteWebServer.GetDecryptedPassword());
                var response = httpClient.Head();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Remote Server responded with an error: " + response.StatusDescription, CheckResult.Fail));
                }
                else
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Confirmed remote server connectivity", CheckResult.Success));                    
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not connect to Remote server '" + RemoteWebServer + "'", CheckResult.Fail, e));
            }

        }

        public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
        {
            extractCommand = value;
        }

        public void PreInitialize(DataLoadInfo value, IDataLoadEventListener listener)
        {
            dataLoadInfo = value;
        }

        public TableLoadInfo TableLoadInfo { get; private set; }

        public string GetDestinationDescription()
        {
            //if there is a cached answer return it
            if (destinationDescription == null)
            {
                var project = extractCommand.Configuration.Project;
                destinationDescription = SqlSyntaxHelper.GetSensibleTableNameFromString(project.Name + "_" + project.ProjectNumber + "_" + extractCommand.Configuration + "_" + extractCommand);
            }

            return destinationDescription;
        }

        public void ExtractGlobals(Project project, ExtractionConfiguration configuration, GlobalsBundle globalsToExtract, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            throw new NotImplementedException();
        }
    }
}