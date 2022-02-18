// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Web
{
    /// <summary>
    /// Data load component which downloads a file from a remote URL (e.g. http) into the ForLoading directory of the load.
    /// </summary>
    public class WebFileDownloader : IPluginDataProvider
    {

        [DemandsInitialization("The full URI to a file that will be downloaded into project ForLoading directory, must be a valid Uri", Mandatory = true)]
        public Uri UriToFile { get; set; }
        
        [DemandsInitialization("Optional Username/password to use for network Websense challenges, these will be provided to the WebRequest as a NetworkCredential")]
        public DataAccessCredentials WebsenseCredentials { get; set; }

        public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            Stopwatch t = new Stopwatch();

            FileInfo destinationFile = new FileInfo(Path.Combine(job.LoadDirectory.ForLoading.FullName, Path.GetFileName(UriToFile.LocalPath)));

            DownloadFileWhilstPretendingToBeFirefox(t, destinationFile,job);

            job.OnProgress(this,new ProgressEventArgs(destinationFile.FullName, new ProgressMeasurement((int)(destinationFile.Length / 1000),ProgressType.Kilobytes), t.Elapsed));

            return ExitCodeType.Success;

        }

        private void DownloadFileWhilstPretendingToBeFirefox(Stopwatch t, FileInfo destinationFile,IDataLoadJob job)
        {
            t.Start();
            HttpWebRequest request = CreateNewRequest(UriToFile.AbsoluteUri);
            
            StreamWriter writer = File.CreateText(destinationFile.FullName);
            
            WebResponse response;
            try
            {
                //make initial web request for the file
                response = request.GetResponse();
            }
            catch (WebException e)
            {
                //Websense Challenge Response
                #region Websense Challenge Response

                //is the reason that we could not get the file because of a websense challenge
                if (e.Response.Headers.AllKeys.Any(h => h.Contains("WWW-Authenticate")))
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            "received " + e.Status +
                            " and a header containing WWW-Authenticate instructions to go to URL " +
                            e.Response.ResponseUri.AbsoluteUri, null));
                else
                    throw;//no something else went wrong

                const string websenseIs = "Basic realm=\"Websense\"";
                if(!e.Response.Headers["WWW-Authenticate"].Equals(websenseIs))
                    throw new Exception("Although we were challenged with a WWW-Authenticate header the content of the header was not '" + websenseIs + "' so we are not going to send it our websense credentials, the challenge header we got contained the following:'" + e.Response.Headers["WWW-Authenticate"] +"'");


                //it is a websense challenge but user doesn't want to respond
                if (WebsenseCredentials == null)
                    throw new Exception("We received a websense challenge but WebsenseCredentials is null so we will not respond and must fail the job, create a new DataAccessCredentials object and associate it with the pipeline component's WebsenseCredentials to get around this problem", e);

                string websenseUsername;
                string websensePassword;

                //user wants to respond to the challenge
                try
                {
                    try
                    {
                        //see if username and password are in the configuration details file (which is under access control on a secure filesystem - hopefully!)
                        websenseUsername = WebsenseCredentials.Username;
                        websensePassword = WebsenseCredentials.GetDecryptedPassword();
                    }
                    catch (Exception exception)
                    {
                        //websense details are missing or corrupt
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Could not authenticate against websense because there was a problem with the WebsenseCredentials of the PipelineComponent", exception));
                        throw;
                    }

                    //websense details are ok

                    //make a request to the websense proxy
                    request = CreateNewRequest(e.Response.ResponseUri.AbsoluteUri);
                    request.Credentials = new NetworkCredential(websenseUsername,websensePassword );
                    response = request.GetResponse();

                    //GetResponse worked from the proxy websense
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Websense authentication worked, preparing to re-send original request", null));
                }
                catch (Exception exception)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,"Exception occurred while trying to authenticate against websense",exception));
                    throw;
                }

                //we have now successfully authenticated against websense (whew!) now we have to resend the original request

                //now make another request for the original page
                request = CreateNewRequest(UriToFile.AbsoluteUri);
                request.Credentials = new NetworkCredential(websenseUsername, websensePassword);
                response = request.GetResponse(); //if it crashes again here then websense timeout must be like 0.1ms!
                #endregion
            }
            

            //download the file 
            using (var responseStream = response.GetResponseStream())
            {
                // Process the stream
                byte[] buf = new byte[16384];

                int countReadSoFar = 0;
                int count = 0;
                do
                {
                    t.Start();
                    count = responseStream.Read(buf, 0, buf.Length);
                    t.Stop();

                    if (count != 0)
                    {
                        writer.Write(Encoding.ASCII.GetString(buf, 0, count));
                        writer.Flush();
                        countReadSoFar += count;

                        job.OnProgress(this,new ProgressEventArgs(destinationFile.FullName, new ProgressMeasurement(countReadSoFar/1000,ProgressType.Kilobytes),t.Elapsed));
                    }
                } while (count > 0);

                responseStream.Close();
                response.Close();
                writer.Close();
            }
            

            t.Stop();
        }

        private HttpWebRequest CreateNewRequest(string url)
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            request.Timeout = 5000; // milliseconds, adjust as needed
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";
            request.ReadWriteTimeout = 10000; // milliseconds, adjust as needed


            return request;
        }

        public string GetDescription()
        {
            throw new System.NotImplementedException();
        }

        public IDataProvider Clone()
        {
            throw new System.NotImplementedException();
        }

        public bool Validate(ILoadDirectory destination)
        {
            if (UriToFile == null || string.IsNullOrWhiteSpace(UriToFile.PathAndQuery))
                throw new MissingFieldException("PathToFile is null or whitespace - should be populated externally as a parameter");
            return true;
        }

        

        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
        }

        
        public void Check(ICheckNotifier notifier)
        {
            if (UriToFile == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No URI has been specified", CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("URI is:" + UriToFile, CheckResult.Success));


        }

    }
}
