// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Renci.SshNet;

namespace Rdmp.Core.DataLoad.Modules.FTP;

/// <summary>
/// load component which downloads files from a remote SFTP (Secure File Transfer Protocol) server to the ForLoading directory
/// 
/// <para>Operates in the same way as <see cref="FTPDownloader"/> except that it uses SSH.  In addition this 
/// class will not bother downloading any files that already exist in the forLoading directory (have the same name - file size is NOT checked)</para>
/// </summary>
public class SFTPDownloader:FTPDownloader
{

    [DemandsInitialization("The keep-alive interval.  In milliseconds.  Requires KeepAlive to be set to take effect.")]
    public int KeepAliveIntervalMilliseconds { get; set; }

    protected override void Download(string file, ILoadDirectory destination,IDataLoadEventListener job)
    {
        if (file.Contains("/") || file.Contains("\\"))
            throw new Exception("Was not expecting a relative path here");
            
        var s = new Stopwatch();
        s.Start();
            
        using(var sftp = new SftpClient(_host,_username,_password))
        {
            if (KeepAlive && KeepAliveIntervalMilliseconds > 0)
            {
                sftp.KeepAliveInterval =   TimeSpan.FromMilliseconds(KeepAliveIntervalMilliseconds);
            }

            sftp.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, TimeoutInSeconds);
            sftp.Connect();
                
            //if there is a specified remote directory then reference it otherwise reference it locally (or however we were told about it from GetFileList())
            var fullFilePath = !string.IsNullOrWhiteSpace(RemoteDirectory) ? Path.Combine(RemoteDirectory, file) : file;
                
            var destinationFilePath = Path.Combine(destination.ForLoading.FullName, file);

            //register for events
            Action<ulong> callback = totalBytes => job.OnProgress(this, new ProgressEventArgs(destinationFilePath, new ProgressMeasurement((int)(totalBytes * 0.001), ProgressType.Kilobytes), s.Elapsed));

            using (var fs = new FileStream(destinationFilePath, FileMode.CreateNew))
            {
                //download
                sftp.DownloadFile(fullFilePath, fs, callback);
                fs.Close();
            }
            _filesRetrieved.Add(fullFilePath);

        }
        s.Stop();
    }


    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        if(exitCode == ExitCodeType.Success)
        {
            using (var sftp = new SftpClient(_host, _username, _password))
            {
                sftp.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, TimeoutInSeconds);
                sftp.Connect();
                    
                foreach (var retrievedFiles in _filesRetrieved)
                    try
                    {
                        sftp.DeleteFile(retrievedFiles);
                        postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                            $"Deleted SFTP file {retrievedFiles} from SFTP server"));
                    }
                    catch (Exception e)
                    {
                        postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                            $"Could not delete SFTP file {retrievedFiles} from SFTP server", e));
                    }
            }
                
        }
    }


    protected override string[] GetFileList()
    {
        using (var sftp = new SftpClient(_host, _username, _password))
        {
            sftp.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, TimeoutInSeconds);
            sftp.Connect();

            var directory = RemoteDirectory;

            if (string.IsNullOrWhiteSpace(directory))
                directory = ".";
                
            return sftp.ListDirectory(directory).Select(d=>d.Name).ToArray();
        }

    }
}