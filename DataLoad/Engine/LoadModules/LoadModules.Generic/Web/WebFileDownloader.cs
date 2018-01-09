using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Web
{
    /// <summary>
    /// Data load component which downloads a file from a remote URL (e.g. http) into the ForLoading directory of the load.
    /// </summary>
    [Description("Connects to the given PathToFile and downloads the file it finds at the url")]
    public class WebFileDownloader : IPluginDataProvider
    {

        [DemandsInitialization("The full URI to a file that will be downloaded into project ForLoading directory, must be a valid Uri", Mandatory = true)]
        public Uri UriToFile { get; set; }

        [DemandsInitialization("Processes challenges from WebSense for credentials to access stuff on the internet.  This requires that you specify a username and password for use with websense in the HICProjectDirectory properties file (ConfigurationDetails.xml) - to do this add the following <WebsenseUsername>ExmapleUsername</WebsenseUsername> and <WebsensePassword>ExamplePassword</WebsensePassword>")]
        public bool DetectWebsenseChallenges { get; set; }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            Stopwatch t = new Stopwatch();

            FileInfo destinationFile = new FileInfo(Path.Combine(job.HICProjectDirectory.ForLoading.FullName, Path.GetFileName(UriToFile.LocalPath)));

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
                if(!DetectWebsenseChallenges)
                    throw new Exception("We received a websense challenge but DetectWebsenseChallenges is false so we will not respond and must fail the job, set ProcessTaskArgument DetectWebsenseChallenges to true and add <WebsenseUsername>ExmapleUsername</WebsenseUsername> and <WebsensePassword>ExamplePassword</WebsensePassword> to the ConfigurationDetails.xml file to get around this problem",e);

                //user wants to respond to the challenge
                try
                {
                    string websenseUsername;
                        string websensePassword;

                    try
                    {
                        //see if username and password are in the configuration details file (which is under access control on a secure filesystem - hopefully!)
                        websenseUsername = job.HICProjectDirectory.GetTagFromConfigurationDataXML("WebsenseUsername")[0].InnerText;
                        websensePassword = job.HICProjectDirectory.GetTagFromConfigurationDataXML("WebsensePassword")[0].InnerText;

                        if(string.IsNullOrEmpty(websenseUsername) || string.IsNullOrWhiteSpace(websensePassword))
                            throw new Exception("WebsenseUsername or WebsensePassword was missing from HICProjectDirectory ConfigurationDataXML or was blank");
                    }
                    catch (Exception exception)
                    {
                        //websense details are missing or corrupt
                        job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Could not authenticate against websense because the ConfigurationDetails.xml file does not have the tags <WebsenseUsername>ExmapleUsername</WebsenseUsername> and <WebsensePassword>ExamplePassword</WebsensePassword>",exception));
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
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
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

        public bool Validate(IHICProjectDirectory destination)
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
