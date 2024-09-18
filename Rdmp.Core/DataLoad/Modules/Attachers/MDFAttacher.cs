// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Data load component for loading a detached database file into RAW.  This attacher does not load RAW tables normally (like AnySeparatorFileAttacher etc)
/// instead it specifies that it is itself going to act as RAW.  Using this component requires that the computer running the data load has file system access
/// to the RAW Sql Server data directory (and that the path is the same).
/// 
/// <para>The mdf file will be copied to the Sql Server data directory of the RAW server and attached with the expected name of RAW.  From this point on the load
/// will function normally.  It is up to the user to ensure that the table names/columns in the attached MDF match expected LIVE tables on your server (or
/// write AdjustRAW scripts to harmonise).</para>
/// </summary>
public class MDFAttacher : Attacher, IPluginAttacher
{
    private const string GetDefaultSQLServerDatabaseDirectory = @"SELECT physical_name 
            FROM sys.master_files mf   
            INNER JOIN sys.[databases] d   
            ON mf.[database_id] = d.[database_id]   
            WHERE d.[name] = 'master' AND type = 0";

    [DemandsInitialization(
        "Set this only if your RAW server is NOT localhost.  This is the network path to the DATA directory of your RAW database server you can find the DATA directory by running 'select * FROM sys.master_files'")]
    public string OverrideMDFFileCopyDestination { get; set; }

    [DemandsInitialization(
        @"There are multiple ways to attach a mdf files to an SQL server, the first stage is always to copy the mdf and ldf files to the DATA directory of your server but after that it gets flexible.  
1. AttachWithConnectionString attempts to do the attaching as part of connection by specifying the AttachDBFilename keyword in the connection string
2. ExecuteCreateDatabaseForAttachSql attempts to connect to 'master' and execute CREATE DATABASE sql with the FILENAME property set to your mdf file in the DATA directory of your database server
If you are attempting to attach an MDF file from a Linux machine to a Window machine, or  vice-versa, you will have to use the ExecuteCreateDatabaseForAttachSql to be able to handle the mismatched directory structure")]
    public MdfAttachStrategy AttachStrategy { get; set; }

    [DemandsInitialization(
        @"Set this only if you encounter problems with the ATTACH stage path.  This is the local path to the .mdf file in the DATA directory from the perspective of SQL Server.
There are a number of variables for use within this override path:
%d : the current date in the month e.g. 04
%m : the current month  e.g. 12
%y : the current year e.e. 24
")]
    public string OverrideAttachMdfPath { get; set; }

    [DemandsInitialization(
        @"Set this only if you encounter problems with the ATTACH stage path.  This is the local path to the .ldf file in the DATA directory from the perspective of SQL Server.
There are a number of variables for use within this override path:
%d : the current date in the month e.g. 04
%m : the current month  e.g. 12
%y : the current year e.e. 2024
")]
    public string OverrideAttachLdfPath { get; set; }

    public MDFAttacher() : base(false)
    {
    }

    private MdfFileAttachLocations _locations;


    private static string ReplacedateVariables(string str)
    {
        return str.Replace("%d", DateTime.Now.ToString("dd")).Replace("%m", DateTime.Now.ToString("MM")).Replace("%y", DateTime.Now.ToString("yy"));
    }

    private void GetFileNames()
    {
        if ((string.IsNullOrWhiteSpace(OverrideAttachLdfPath) || OverrideAttachLdfPath.EndsWith(".ldf")) && (string.IsNullOrWhiteSpace(OverrideAttachMdfPath) || OverrideAttachMdfPath.EndsWith(".mdf"))) return;//don't need to fiddle with the paths
        var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = 600
        };

        using var con = new SqlConnection(builder.ConnectionString);
        con.Open();
        using var dt = new DataTable();
        using (var cmd = DatabaseCommandHelper.GetCommand($"DBCC CHECKPRIMARYFILE (N'{_locations.AttachMdfPath}' , 3)", con))
        using (var da = DatabaseCommandHelper.GetDataAdapter(cmd))
        {
            dt.BeginLoadData();
            da.Fill(dt);
            dt.EndLoadData();
        }

        if (!string.IsNullOrWhiteSpace(OverrideAttachLdfPath) && !OverrideAttachLdfPath.EndsWith(".ldf", StringComparison.OrdinalIgnoreCase))
        {
            var _path = dt.Rows[1].ItemArray[3].ToString();
            var path = ReplacedateVariables(OverrideAttachLdfPath);
            _locations.AttachLdfPath = MdfFileAttachLocations.MergeDirectoryAndFileUsingAssumedDirectorySeparator(path, _path); 
        }
        else
        {
            _locations.AttachLdfPath = OverrideAttachLdfPath;
        }
        if (!string.IsNullOrWhiteSpace(OverrideAttachMdfPath) && !OverrideAttachMdfPath.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
        {
            var _path = dt.Rows[0].ItemArray[3].ToString();
            var path = ReplacedateVariables(OverrideAttachMdfPath);
            _locations.AttachMdfPath = MdfFileAttachLocations.MergeDirectoryAndFileUsingAssumedDirectorySeparator(path, _path);
        }
        else
        {
            _locations.AttachMdfPath = OverrideAttachMdfPath;
        }
    }

    public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        //The location of .mdf files from the perspective of the database server
        var databaseDirectory =
            FindDefaultSQLServerDatabaseDirectory(new FromDataLoadEventListenerToCheckNotifier(job));
        _locations =
            new MdfFileAttachLocations(LoadDirectory.ForLoading, databaseDirectory, OverrideMDFFileCopyDestination);

        GetFileNames();

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Identified the MDF file:{_locations.OriginLocationMdf} and corresponding LDF file:{_locations.OriginLocationLdf}"));

        AsyncCopyMDFFilesWithEvents(_locations.OriginLocationMdf, _locations.CopyToMdf, _locations.OriginLocationLdf,
            _locations.CopyToLdf, job);

        return AttachStrategy switch
        {
            MdfAttachStrategy.AttachWithConnectionString => AttachWithConnectionString(job),
            MdfAttachStrategy.ExecuteCreateDatabaseForAttachSql => ExecuteCreateDatabaseForAttachSql(job),
            _ => throw new ArgumentOutOfRangeException(nameof(AttachStrategy))
        };
    }

    private ExitCodeType ExecuteCreateDatabaseForAttachSql(IDataLoadEventListener listener)
    {
        // connect to master
        var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = 600
        };

        using var con = new SqlConnection(builder.ConnectionString);
        try
        {
            if (_dbInfo.Exists())
                throw new Exception(
                    $"Database {_dbInfo.GetRuntimeName()} already exists on server {builder.DataSource}");

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"About to connect to master on {builder.DataSource}"));
            con.Open();

            var nameTheyWant = _dbInfo.GetRuntimeName();

            var cmd = new SqlCommand($@"  CREATE DATABASE {nameTheyWant}   
   ON (FILENAME = '{_locations.AttachMdfPath}'),   
   (FILENAME = '{_locations.AttachLdfPath}')   
   FOR ATTACH;  ", con);


            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"About to execute SQL: {cmd.CommandText}"));

            cmd.ExecuteNonQuery();

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"SQL completed successfully: {cmd.CommandText}"));

            if (!_dbInfo.Exists())
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Database {_dbInfo.GetRuntimeName()} attach SQL worked but it is still showing up as not existing..."));
                return ExitCodeType.Error;
            }

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Database {_dbInfo.GetRuntimeName()} now exists"));
        }
        catch (Exception e)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Could not attach file {_locations.AttachMdfPath} to database", e));

            return ExitCodeType.Error;
        }

        return ExitCodeType.Success;
    }

    private ExitCodeType AttachWithConnectionString(IDataLoadEventListener listener)
    {
        // Attach database
        var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString)
        {
            AttachDBFilename = _locations.AttachMdfPath,
            InitialCatalog = _dbInfo.GetRuntimeName(),
            ConnectTimeout = 600
        };

        using var attachConnection = new SqlConnection(builder.ConnectionString);
        try
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"About to attach file {_locations.AttachMdfPath} as a database to server {builder.DataSource}"));
            attachConnection.Open();
        }
        catch (Exception e)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Could not attach file {_locations.AttachMdfPath} to database", e));
            return ExitCodeType.Error;
        }

        return ExitCodeType.Success;
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        //don't bother cleaning up if it bombed
        if (exitCode == ExitCodeType.Error)
            return;

        //its Abort,Success or LoadNotRequired

        // Detach database
        try
        {
            var dbToDropName = _dbInfo.GetRuntimeName();

            if (!dbToDropName.EndsWith("_RAW"))
                throw new Exception(
                    $"We were in the cleanup phase and were about to drop the database that was created by MDFAttacher when we noticed its name didn't end with _RAW!, its name was:{dbToDropName} were we about to nuke your live database?");

            _dbInfo.Drop();

            // Only delete the copied file if we successfully extracted the content first, to save ChrisH lots of re-copying...
            if (exitCode == ExitCodeType.Success)
                DeleteFilesIfExist();
        }
        catch (Exception e)
        {
            throw new Exception($"Could not detach database '{_dbInfo.GetRuntimeName()}': {e}");
        }
    }

    private void DeleteFilesIfExist()
    {
        if (File.Exists(_locations.CopyToLdf))
            File.Delete(_locations.CopyToLdf);

        if (File.Exists(_locations.CopyToMdf))
            File.Delete(_locations.CopyToMdf);
    }

    /// <summary>
    /// Determine if two files are 'similar' - timestamps, sizes, first and last 4K
    /// </summary>
    /// <param name="pathA"></param>
    /// <param name="pathB"></param>
    /// <param name="job"></param>
    /// <returns></returns>
    private bool FilesSimilar(string pathA, string pathB, IDataLoadEventListener job)
    {
        try
        {
            var bufferA = new byte[4096];
            var bufferB = new byte[4096];
            var a = new FileInfo(pathA);
            var b = new FileInfo(pathB);
            if (!a.Exists || !b.Exists) return false;
            if (a.LastWriteTimeUtc != b.LastWriteTimeUtc) return false;
            if (a.Length != b.Length) return false;
            if (a.Length < 8192) return false;
            using var streamA = File.OpenRead(pathA);
            using var streamB = File.OpenRead(pathB);
            if (streamA.Read(bufferA, 0, 4096) != 4096 || streamB.Read(bufferB, 0, 4096) != 4096) return false;
            if (!bufferA.SequenceEqual(bufferB)) return false;
            streamA.Seek(-4096, SeekOrigin.End);
            streamB.Seek(-4096, SeekOrigin.End);
            if (streamA.Read(bufferA, 0, 4096) != 4096 || streamB.Read(bufferB, 0, 4096) != 4096) return false;
            return bufferA.SequenceEqual(bufferB);
        }
        catch (Exception e)
        {
            job.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    $"Unable to compare files {pathA} and {pathB} due to {e.Message}", e));
            return false;
        }
    }

    private void CopyIfNeeded(string src, string dest, IDataLoadEventListener job)
    {
        if (FilesSimilar(src, dest, job))
        {
            job.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information, $"Files {src} and {dest} match, skipping copy"));
        }
        else
        {
            if (File.Exists(dest))
                job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Warning, "Overwriting existing database file '{dest}'"));
            File.Copy(src, dest, true);
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Copied {src} to {dest}"));
        }
    }

    private void AsyncCopyMDFFilesWithEvents(string MDFSource, string MDFDestination, string LDFSource,
        string LDFDestination, IDataLoadEventListener job)
    {
        ArgumentNullException.ThrowIfNull(MDFDestination);
        ArgumentNullException.ThrowIfNull(LDFDestination);

        CopyIfNeeded(MDFSource, MDFDestination, job);
        CopyIfNeeded(LDFSource, LDFDestination, job);
    }

    public string FindDefaultSQLServerDatabaseDirectory(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(new CheckEventArgs("About to look up Sql Server DATA directory Path",
            CheckResult.Success));

        try
        {
            //connect to master to run the data directory discovery SQL
            var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            notifier.OnCheckPerformed(new CheckEventArgs($"About to run:\r\n{GetDefaultSQLServerDatabaseDirectory}",
                CheckResult.Success));

            using var cmd = new SqlCommand(GetDefaultSQLServerDatabaseDirectory, connection);
            var result = cmd.ExecuteScalar() as string;

            if (string.IsNullOrWhiteSpace(result))
                throw new Exception(
                    "Looking up DATA directory on server returned null (user may not have permissions to read from relevant sys tables)");

            var end = result.LastIndexOfAny(@"\/".ToCharArray());
            return end == -1
                ? throw new Exception($"No directory delimiter found in DB file location '{result}'")
                : result[..end];
        }
        catch (SqlException e)
        {
            throw new Exception($"Could not execute the command: {GetDefaultSQLServerDatabaseDirectory}", e);
        }
    }

    public override void Check(ICheckNotifier notifier)
    {
        var localSqlServerDataDirectory = !string.IsNullOrWhiteSpace(OverrideMDFFileCopyDestination)
            ? OverrideMDFFileCopyDestination
            : FindDefaultSQLServerDatabaseDirectory(notifier);

        var mdfFilename = $"{_dbInfo.GetRuntimeName()}.mdf";
        var ldfFilename = $"{_dbInfo.GetRuntimeName()}_log.ldf";


        if (Directory.Exists(localSqlServerDataDirectory))
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Found server DATA folder (that we will copy mdf/ldf files to at path:{localSqlServerDataDirectory}",
                    CheckResult.Success));
        else
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Proposed server DATA folder (that we will copy mdf/ldf files to) was not found, proposed path was:{localSqlServerDataDirectory}",
                    CheckResult.Fail));

        if (File.Exists(Path.Combine(localSqlServerDataDirectory, mdfFilename)))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"The database file '{mdfFilename}' exists in the local SQL server data directory '{localSqlServerDataDirectory}'. A database called '{_dbInfo.GetRuntimeName()}' may already be attached, which will cause the load process to fail. Delete this file to continue.",
                CheckResult.Fail, null, "Delete file"));

        if (File.Exists(Path.Combine(localSqlServerDataDirectory, ldfFilename)))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"The database log file '{ldfFilename}' exists in the local SQL server data directory '{localSqlServerDataDirectory}'. A database called '{_dbInfo.GetRuntimeName()}'may already be attached, which will cause the load process to fail. Delete this file to continue.",
                CheckResult.Fail, null, "Delete file"));
    }
}