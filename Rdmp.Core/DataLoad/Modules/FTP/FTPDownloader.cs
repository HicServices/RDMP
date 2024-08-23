// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using FAnsi.Discovery;
using FluentFTP;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.FTP;

/// <summary>
/// load component which downloads files from a remote FTP server to the ForLoading directory
/// 
/// <para>Attempts to connect to the FTP server and download all files in the landing folder of the FTP (make sure you really want everything in the
///  root folder - if not then configure redirection on the FTP, so you land in the correct directory).  Files are downloaded into the ForLoading folder</para>
/// </summary>
public class FTPDownloader : IPluginDataProvider
{
    private readonly Lazy<FtpClient> _connection;
    protected readonly List<string> _filesRetrieved = new();
    private ILoadDirectory? _directory;

    public FTPDownloader()
    {
        _connection = new Lazy<FtpClient>(SetupFtp, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    [DemandsInitialization(
        "Determines the behaviour of the system when no files are found on the server.  If true the entire data load process immediately stops with exit code LoadNotRequired, if false then the load proceeds as normal (useful if for example if you have multiple Attachers and some files are optional)")]
    public bool SendLoadNotRequiredIfFileNotFound { get; set; }

    [DemandsInitialization(
        "The Regex expression to validate files on the FTP server against, only files matching the expression will be downloaded")]
    public Regex? FilePattern { get; set; }

    [DemandsInitialization("The timeout to use when connecting to the FTP server in SECONDS")]
    public int TimeoutInSeconds { get; set; }

    [DemandsInitialization(
        "Tick to delete files from the FTP server when the load is successful (ends with .Success not .OperationNotRequired - which happens when LoadNotRequired state).  This will only delete the files if they were actually fetched from the FTP server.  If the files were already in forLoading then the remote files are not deleted")]
    public bool DeleteFilesOffFTPServerAfterSuccesfulDataLoad { get; set; }

    [DemandsInitialization(
        "The FTP server to connect to.  Server should be specified with only IP:Port e.g. 127.0.0.1:20.  You do not have to specify ftp:// at the start",
        Mandatory = true)]
    public ExternalDatabaseServer? FTPServer { get; set; }

    [DemandsInitialization("The directory on the FTP server that you want to download files from")]
    public string? RemoteDirectory { get; set; }

    [DemandsInitialization("True to set keep alive", DefaultValue = true)]
    public bool KeepAlive { get; set; }


    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
        _directory = directory;
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        return DownloadFilesOnFTP(_directory ?? throw new InvalidOperationException("No output directory set"), job);
    }

    private FtpClient SetupFtp()
    {
        var host = FTPServer?.Server ?? throw new NullReferenceException("FTP server not set");
        var username = FTPServer.Username ?? "anonymous";
        var password = string.IsNullOrWhiteSpace(FTPServer.Password) ? "guest" : FTPServer.GetDecryptedPassword();
        var c = new FtpClient(host, username, password);
        if (TimeoutInSeconds > 0)
        {
            c.Config.ConnectTimeout = TimeoutInSeconds * 1000;
            c.Config.ReadTimeout = TimeoutInSeconds * 1000;
            c.Config.DataConnectionConnectTimeout = TimeoutInSeconds * 1000;
            c.Config.DataConnectionReadTimeout = TimeoutInSeconds * 1000;
        }
        // Enable periodic NOOP keepalive operations to keep connection active until we're done
        c.Config.Noop = true;
        c.AutoConnect();

        return c;
    }

    private ExitCodeType DownloadFilesOnFTP(ILoadDirectory destination, IDataLoadEventListener listener)
    {
        var files = GetFileList().ToArray();

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Identified the following files on the FTP server:{string.Join(',', files)}"));

        var forLoadingContainedCachedFiles = false;

        foreach (var file in files)
        {
            var action = GetSkipActionForFile(file, destination);

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"File {file} was evaluated as {action}"));

            switch (action)
            {
                case SkipReason.DoNotSkip:
                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information, $"About to download {file}"));
                    Download(file, destination);
                    break;
                case SkipReason.InForLoading:
                    forLoadingContainedCachedFiles = true;
                    break;
            }
        }

        // it was a success - even if no files were actually retrieved... hey that's what the user said, otherwise he would have set SendLoadNotRequiredIfFileNotFound
        if (forLoadingContainedCachedFiles || _filesRetrieved.Count != 0 || !SendLoadNotRequiredIfFileNotFound)
            return ExitCodeType.Success;

        // if no files were downloaded (and there were none skipped because they were in forLoading) and in that eventuality we have our flag set to return LoadNotRequired then do so
        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                "Could not find any files on the remote server worth downloading, so returning LoadNotRequired"));
        return ExitCodeType.OperationNotRequired;
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
        if (file.StartsWith(".", StringComparison.Ordinal))
            return SkipReason.IsImaginaryFile;

        //if there is a regex pattern
        if (FilePattern?.IsMatch(file) == false) //and it does not match
            return SkipReason.DidNotMatchPattern; //skip because it did not match pattern

        //if the file on the FTP already exists in the forLoading directory, skip it
        return destination.ForLoading.GetFiles(file).Any() ? SkipReason.InForLoading : SkipReason.DoNotSkip;
    }


    private static bool ValidateServerCertificate(object _1, X509Certificate _2, X509Chain _3,
        SslPolicyErrors _4) => true; //any cert will do! yay


    protected virtual IEnumerable<string> GetFileList()
    {
        return _connection.Value.GetNameListing().ToList().Where(_connection.Value.FileExists);
    }

    protected virtual void Download(string file, ILoadDirectory destination)
    {
        var remotePath = !string.IsNullOrWhiteSpace(RemoteDirectory)
            ? $"{RemoteDirectory}/{file}"
            : file;

        var destinationFileName = Path.Combine(destination.ForLoading.FullName, file);
        _connection.Value.DownloadFile(destinationFileName, remotePath);
        _filesRetrieved.Add(remotePath);
    }

    public virtual void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        if (exitCode != ExitCodeType.Success || !DeleteFilesOffFTPServerAfterSuccesfulDataLoad) return;

        // Force a reconnection attempt if we got cut off
        if (!_connection.Value.IsStillConnected())
            _connection.Value.Connect(true);
        foreach (var file in _filesRetrieved) _connection.Value.DeleteFile(file);
    }


    public virtual void Check(ICheckNotifier notifier)
    {
        try
        {
            SetupFtp();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to SetupFTP", CheckResult.Fail, e));
        }
    }
}