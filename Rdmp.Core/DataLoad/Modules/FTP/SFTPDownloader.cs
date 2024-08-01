// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Renci.SshNet;

namespace Rdmp.Core.DataLoad.Modules.FTP;

/// <summary>
/// load component which downloads files from a remote SFTP (Secure File Transfer Protocol) server to the ForLoading directory
/// 
/// <para>Operates in the same way as <see cref="FTPDownloader"/> except that it uses SSH.  In addition, this
/// class will not bother downloading any files that already exist in the forLoading directory (have the same name - file size is NOT checked)</para>
/// </summary>
public class SFTPDownloader : FTPDownloader
{
    [DemandsInitialization("The keep-alive interval.  In milliseconds.  Requires KeepAlive to be set to take effect.")]
    public int KeepAliveIntervalMilliseconds { get; set; }

    private readonly Lazy<SftpClient> _connection;

    public SFTPDownloader(Lazy<SftpClient> connection)
    {
        _connection = new Lazy<SftpClient>(SetupSftp, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public SFTPDownloader()
    {
        _connection = new Lazy<SftpClient>(SetupSftp, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private SftpClient SetupSftp()
    {
        var host = FTPServer?.Server ?? throw new NullReferenceException("FTP server not set");
        var username = FTPServer.Username ?? "anonymous";
        var password = string.IsNullOrWhiteSpace(FTPServer.Password) ? "guest" : FTPServer.GetDecryptedPassword();
        var c = new SftpClient(host, username, password);
        if (TimeoutInSeconds > 0)
            c.OperationTimeout = new TimeSpan(10000 * TimeoutInSeconds);
            c.ConnectionInfo.Timeout = new TimeSpan(10000 * TimeoutInSeconds);
        c.Connect();
        if (KeepAlive)
            c.KeepAliveInterval = TimeSpan.FromMilliseconds(KeepAliveIntervalMilliseconds);
        return c;
    }

    public override void Check(ICheckNotifier notifier)
    {
        try
        {
            SetupSftp();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to SetupSFTP", CheckResult.Fail, e));
        }
    }


    protected override void Download(string file, ILoadDirectory destination)
    {
        if (file.Contains('/') || file.Contains('\\'))
            throw new Exception("Was not expecting a relative path here");

        //if there is a specified remote directory then reference it otherwise reference it locally (or however we were told about it from GetFileList())
        var fullFilePath = !string.IsNullOrWhiteSpace(RemoteDirectory) ? Path.Combine(RemoteDirectory, file) : file;

        var destinationFilePath = Path.Combine(destination.ForLoading.FullName, file);

        using (var dest = File.Create(destinationFilePath))
            _connection.Value.DownloadFile(fullFilePath, dest);
        _filesRetrieved.Add(fullFilePath);
    }


    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        if (exitCode != ExitCodeType.Success) return;

        // Reconnect if we got cut off, for example due to idle timers
        if (!_connection.Value.IsConnected)
            _connection.Value.Connect();

        foreach (var retrievedFiles in _filesRetrieved)
            try
            {
                _connection.Value.DeleteFile(retrievedFiles);
                postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Deleted SFTP file {retrievedFiles} from SFTP server"));
            }
            catch (Exception e)
            {
                postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Could not delete SFTP file {retrievedFiles} from SFTP server", e));
            }
    }


    protected override string[] GetFileList()
    {
        var directory = string.IsNullOrWhiteSpace(RemoteDirectory) ? "." : RemoteDirectory;

        return _connection.Value.ListDirectory(directory).Select(static d => d.Name).ToArray();
    }
}