// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.FTP;

/// <summary>
/// load component which downloads files from a remote FTP server to the ForLoading directory
/// 
/// <para>Attempts to connect to the FTP server and download all files in the landing folder of the FTP (make sure you really want everything in the
///  root folder - if not then configure redirection on the FTP so you land in the correct directory).  Files are downloaded into the ForLoading folder</para>
/// </summary>
public class FTPDownloader : IPluginDataProvider
{
    protected string _host;
    protected string _username;
    protected string _password;
        
    private bool _useSSL = false;

    protected List<string> _filesRetrieved = new List<string>();
    private ILoadDirectory _directory;

    [DemandsInitialization("Determines the behaviour of the system when no files are found on the server.  If true the entire data load process immediately stops with exit code LoadNotRequired, if false then the load proceeds as normal (useful if for example if you have multiple Attachers and some files are optional)")]
    public bool SendLoadNotRequiredIfFileNotFound { get; set; }
        
    [DemandsInitialization("The Regex expression to validate files on the FTP server against, only files matching the expression will be downloaded")]
    public Regex FilePattern { get; set; }

    [DemandsInitialization("The timeout to use when connecting to the FTP server in SECONDS")]
    public int TimeoutInSeconds { get; set; }

    [DemandsInitialization("Tick to delete files from the FTP server when the load is succesful (ends with .Success not .OperationNotRequired - which happens when LoadNotRequired state).  This will only delete the files if they were actually fetched from the FTP server.  If the files were already in forLoading then the remote files are not deleted")]
    public bool DeleteFilesOffFTPServerAfterSuccesfulDataLoad { get; set; }

    [DemandsInitialization("The FTP server to connect to.  Server should be specified with only IP:Port e.g. 127.0.0.1:20.  You do not have to specify ftp:// at the start",Mandatory=true)]
    public ExternalDatabaseServer FTPServer { get; set; }

    [DemandsInitialization("The directory on the FTP server that you want to download files from")]
    public string RemoteDirectory { get; set; }

    [DemandsInitialization("True to set keep alive", DefaultValue = true)]
    public bool KeepAlive { get; set; }


    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
        _directory = directory;
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        SetupFTP();
        return DownloadFilesOnFTP(_directory, job);
    }

    public string GetDescription()
    {
        return "See Description attribute of class";
    }

    public IDataProvider Clone()
    {
        return new FTPDownloader();
    }

    public bool Validate(ILoadDirectory destination)
    {
        SetupFTP();
        return GetFileList().Any();
    }

    private void SetupFTP()
    {
        _host = FTPServer.Server;
        _username = FTPServer.Username ?? "anonymous";
        _password = String.IsNullOrWhiteSpace(FTPServer.Password) ? "guest" : FTPServer.GetDecryptedPassword();

        if(string.IsNullOrWhiteSpace(_host))
            throw new NullReferenceException("FTPServer is not set up correctly it must have Server property filled in" + FTPServer);
    }

    private ExitCodeType DownloadFilesOnFTP(ILoadDirectory destination, IDataLoadEventListener listener)
    {
        string[] files = GetFileList();

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, files.Aggregate("Identified the following files on the FTP server:", (s, f) => f + ",").TrimEnd(',')));
            
        bool forLoadingContainedCachedFiles = false;

        foreach (string file in files)
        {
            var action = GetSkipActionForFile(file, destination);

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "File " + file + " was evaluated as " + action));
            if(action == SkipReason.DoNotSkip)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to download "+file));
                Download(file, destination, listener);
            }

            if (action == SkipReason.InForLoading)
                forLoadingContainedCachedFiles = true;
        }

        //if no files were downloaded (and there were none skiped because they were in forLoading) and in that eventuality we have our flag set to return LoadNotRequired then do so
        if (!forLoadingContainedCachedFiles && !_filesRetrieved.Any() && SendLoadNotRequiredIfFileNotFound)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Could not find any files on the remote server worth downloading, so returning LoadNotRequired"));
            return ExitCodeType.OperationNotRequired;
        }

        //otherwise it was a success - even if no files were actually retrieved... hey that's what the user said, otherwise he would have set SendLoadNotRequiredIfFileNotFound
        return ExitCodeType.Success;
    }

    protected enum SkipReason
    {
        DoNotSkip,
        InForLoading,
        DidNotMatchPattern,
        IsImaginaryFile
    }

    protected SkipReason GetSkipActionForFile(string file, ILoadDirectory destination)
    {
        if (file.StartsWith("."))
            return SkipReason.IsImaginaryFile;

        //if there is a regex pattern
        if(FilePattern != null)
            if (!FilePattern.IsMatch(file))//and it does not match
                return SkipReason.DidNotMatchPattern; //skip because it did not match pattern

        //if the file on the FTP already exists in the forLoading directory, skip it
        if (destination.ForLoading.GetFiles(file).Any())
            return SkipReason.InForLoading;

         
        return SkipReason.DoNotSkip;
    }


    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
    {
        return true;//any cert will do! yay
    }

        
    protected virtual string[] GetFileList()
    {
        StringBuilder result = new StringBuilder();
        WebResponse response = null;
        StreamReader reader = null;
        try
        {
            FtpWebRequest reqFTP;

            string uri;


            if (!string.IsNullOrWhiteSpace(RemoteDirectory))
                uri = "ftp://" + _host + "/" + RemoteDirectory;
            else
                uri = "ftp://" + _host;

#pragma warning disable SYSLIB0014 // Type or member is obsolete
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(_username, _password);
            reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
            reqFTP.Timeout = TimeoutInSeconds*1000;
            reqFTP.KeepAlive = KeepAlive;
                
            reqFTP.Proxy = null;
            reqFTP.KeepAlive = false;
            reqFTP.UsePassive = true;
            reqFTP.EnableSsl = _useSSL;

            //accept any certificates
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
            response = reqFTP.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Append(line);
                result.Append("\n");
                line = reader.ReadLine();
            }
            // to remove the trailing '\n'
            result.Remove(result.ToString().LastIndexOf('\n'), 1);
            return result.ToString().Split('\n');
        }
        finally
        {
            if (reader != null)
                reader.Close();

            if (response != null)
                response.Close();
        }
    }

    protected virtual void Download(string file, ILoadDirectory destination,IDataLoadEventListener job)
    {

        Stopwatch s = new Stopwatch();
        s.Start();

        string uri;
        if (!string.IsNullOrWhiteSpace(RemoteDirectory))
            uri = "ftp://" + _host + "/" + RemoteDirectory + "/" + file;
        else
            uri = "ftp://" + _host + "/" + file;

        if (_useSSL)
            uri = "s" + uri;

        Uri serverUri = new Uri(uri);
        if (serverUri.Scheme != Uri.UriSchemeFtp)
        {
            return;
        }

        FtpWebRequest reqFTP;
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        reqFTP.Credentials = new NetworkCredential(_username, _password);
        reqFTP.KeepAlive = false;
        reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
        reqFTP.UseBinary = true;
        reqFTP.Proxy = null;
        reqFTP.UsePassive = true;
        reqFTP.EnableSsl = _useSSL;
        reqFTP.Timeout = TimeoutInSeconds*1000;

        FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
        Stream responseStream = response.GetResponseStream();
        string destinationFileName = Path.Combine(destination.ForLoading.FullName, file);
            
        using (FileStream writeStream = new FileStream(destinationFileName, FileMode.Create))
        {
            int Length = 2048;
            Byte[] buffer = new Byte[Length];
            int bytesRead = responseStream.Read(buffer, 0, Length);
            int totalBytesReadSoFar = bytesRead;

            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = responseStream.Read(buffer, 0, Length);


                //notify whoever is listening of how far along the process we are
                totalBytesReadSoFar += bytesRead;
                job.OnProgress(this, new ProgressEventArgs(destinationFileName, new ProgressMeasurement(totalBytesReadSoFar / 1024, ProgressType.Kilobytes), s.Elapsed));
            }
            writeStream.Close();
        }
            
        response.Close();

        _filesRetrieved.Add(serverUri.ToString());
        s.Stop();
    }

    public virtual void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
    {

        if (exitCode == ExitCodeType.Success && DeleteFilesOffFTPServerAfterSuccesfulDataLoad)
        {
            foreach (string file in _filesRetrieved)
            {
                FtpWebRequest reqFTP;
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(file));
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                reqFTP.Credentials = new NetworkCredential(_username, _password);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.UsePassive = true;
                reqFTP.EnableSsl = _useSSL;

                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                if(response.StatusCode != FtpStatusCode.FileActionOK)
                    postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Attempt to delete file at URI " + file + " resulted in response with StatusCode = " + response.StatusCode));
                else
                    postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleted FTP file at URI " + file + " status code was " + response.StatusCode));

                response.Close();
            }
        }
    }

        
    public void Check(ICheckNotifier notifier)
    {
        try
        {
            SetupFTP();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to SetupFTP", CheckResult.Fail, e));
        }
    }
}