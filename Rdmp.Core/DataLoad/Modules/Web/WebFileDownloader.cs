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
using System.Net.Http;
using System.Text;
using CsvHelper;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using MissingFieldException = System.MissingFieldException;

namespace Rdmp.Core.DataLoad.Modules.Web;

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
        var t = Stopwatch.StartNew();
        var destinationFile = new FileInfo(Path.Combine(job.LoadDirectory.ForLoading.FullName, Path.GetFileName(UriToFile.LocalPath)));
        DownloadFileWhilstPretendingToBeFirefox(destinationFile,job);
        job.OnProgress(this,new ProgressEventArgs(destinationFile.FullName, new ProgressMeasurement((int)(destinationFile.Length / 1000),ProgressType.Kilobytes), t.Elapsed));
        return ExitCodeType.Success;

    }

    private void DownloadFileWhilstPretendingToBeFirefox(FileInfo destinationFile,IDataLoadJob job)
    {
        NetworkCredential credentials;
        try
        {
            credentials = new NetworkCredential(WebsenseCredentials.Username, WebsenseCredentials.GetDecryptedPassword());
        }
        catch (Exception)
        {
            credentials = null;
        }
        using var response = CreateNewRequest(UriToFile.AbsoluteUri,credentials);
        using var writer = File.Create(destinationFile.FullName);
        //download the file 
        response.CopyTo(writer,1<<20);
    }

    private static Stream CreateNewRequest(string url,ICredentials credentials=null,bool useCredentials=false)
    {
        using var httpClientHandler = new HttpClientHandler();
        if (useCredentials && credentials is not null)
            httpClientHandler.Credentials = credentials;
        using var httpClient = new HttpClient(httpClientHandler, false)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36");
        using var response= httpClient.GetAsync(url).Result;
        if (response.IsSuccessStatusCode)
            return response.Content.ReadAsStreamAsync().Result;
        if (!useCredentials && response.Headers.WwwAuthenticate.Any(h => h.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) && h.Parameter?.Equals("realm=\"Websense\"",StringComparison.OrdinalIgnoreCase)==true))
        {
            return CreateNewRequest(response.Headers.Location?.AbsoluteUri, credentials, true);
        }
        throw new Exception($"Could not get response from {url} - {response.StatusCode} - {response.ReasonPhrase}");
    }

    public string GetDescription()
    {
        throw new NotImplementedException();
    }

    public IDataProvider Clone()
    {
        throw new NotImplementedException();
    }

    public bool Validate(ILoadDirectory _)
    {
        if (string.IsNullOrWhiteSpace(UriToFile?.PathAndQuery))
            throw new MissingFieldException("PathToFile is null or white space - should be populated externally as a parameter");
        return true;
    }

        

    public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
    {
    }

        
    public void Check(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(UriToFile == null
            ? new CheckEventArgs("No URI has been specified", CheckResult.Fail)
            : new CheckEventArgs($"URI is:{UriToFile}", CheckResult.Success));
    }

}