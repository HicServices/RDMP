// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Threading;
using FAnsi.Connections;
using FAnsi.Discovery;

namespace Rdmp.Core.Logging;

/// <summary>
///     A 'table' that is being loaded as part of a logged activity (See DataLoadInfo).  While it is called a table you can
///     actually audit any endpoint for records
///     e.g. a targetTable of 'bob.csv' would be absolutely fine.  As long as the count of inserts is useful and you want
///     to preserve the information then go ahead
///     and create a TableLoadInfo.
///     <para>
///         You can increment Inserts / Deletes etc as often as you want but do not decrease them.  When you are sure you
///         have finished loading the target table
///         (even if there were errors) you should call CloseAndArchive to write the final insert/update/delete count into
///         the database.  After this is called you
///         won't be able to change the counts any more.
///     </para>
/// </summary>
public class TableLoadInfo : ITableLoadInfo
{
    public bool IsClosed { get; private set; }

    private int _errorRows; // incremented only through RowErrorLogging class
    public DataLoadInfo DataLoadInfoParent { get; private set; }

    public DataSource[] DataSources { get; internal set; }


    #region Database Connection Setup

    public DiscoveredServer DatabaseSettings { get; }

    #endregion

    /// <param name="parent"></param>
    /// <param name="suggestedRollbackCommand">
    ///     Human readable text indicating how this load might be rolled back, may contain
    ///     specific SQL or just general advice.
    /// </param>
    /// <param name="destinationTable"></param>
    /// <param name="sources"></param>
    /// <param name="expectedInserts"></param>
    public TableLoadInfo(DataLoadInfo parent, string suggestedRollbackCommand, string destinationTable,
        DataSource[] sources, int expectedInserts)
    {
        DatabaseSettings = parent.DatabaseSettings;

        StartTime = DateTime.Now;
        Inserts = 0;
        Updates = 0;
        Deletes = 0;
        DiscardedDuplicates = 0;
        _errorRows = 0;

        SuggestedRollbackCommand = suggestedRollbackCommand;

        DataLoadInfoParent = parent;

        var md5Col = DatabaseSettings.GetCurrentDatabase().ExpectTable("DataSource").DiscoverColumn("MD5");

        IsLegacyLoggingSchema = md5Col.DataType.SQLType.Contains("binary");

        RecordNewTableLoadInDatabase(parent, destinationTable, sources, expectedInserts);

        parent.AddTableLoad(this);
    }

    private void RecordNewTableLoadInDatabase(DataLoadInfo parent, string destinationTable, DataSource[] sources,
        int expectedInserts)
    {
        using var con = DatabaseSettings.GetConnection();
        using var cmd = DatabaseSettings.GetCommand(
            "INSERT INTO TableLoadRun (startTime,dataLoadRunID,targetTable,expectedInserts,suggestedRollbackCommand) " +
            "VALUES (@startTime,@dataLoadRunID,@targetTable,@expectedInserts,@suggestedRollbackCommand); " +
            "SELECT @@IDENTITY;", con);
        con.Open();

        DatabaseSettings.AddParameterWithValueToCommand("@startTime", cmd, DateTime.Now);
        DatabaseSettings.AddParameterWithValueToCommand("@dataLoadRunID", cmd, parent.ID);
        DatabaseSettings.AddParameterWithValueToCommand("@targetTable", cmd,
            destinationTable.Substring(Math.Max(0,
                destinationTable.Length - 200))); //200 char limit on this table, just pull the rightmost 200 chars
        DatabaseSettings.AddParameterWithValueToCommand("@expectedInserts", cmd, expectedInserts);
        DatabaseSettings.AddParameterWithValueToCommand("@suggestedRollbackCommand", cmd, SuggestedRollbackCommand);


        //get the ID, can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
        ID = int.Parse(cmd.ExecuteScalar().ToString());


        //keep a record of all data sources
        DataSources = sources;

        //for each of the sources, create them in the DataSource table
        foreach (var s in DataSources)
        {
            using var cmdInsertDs = DatabaseSettings.GetCommand(
                "INSERT INTO DataSource (source,tableLoadRunID,originDate,MD5) " +
                "VALUES (@source,@tableLoadRunID,@originDate,@MD5); SELECT @@IDENTITY;", con);
            DatabaseSettings.AddParameterWithValueToCommand("@source", cmdInsertDs, s.Source);
            DatabaseSettings.AddParameterWithValueToCommand("@tableLoadRunID", cmdInsertDs, ID);
            DatabaseSettings.AddParameterWithValueToCommand("@originDate", cmdInsertDs,
                s.UnknownOriginDate ? DBNull.Value : s.OriginDate);

            // old logging schema used binary[128] for the MD5 column
            if (IsLegacyLoggingSchema)
            {
                var p = cmdInsertDs.CreateParameter();
                p.DbType = DbType.Binary;
                p.Size = 128;
                p.Value = s.MD5 != null ? s.MD5 : DBNull.Value;
                p.ParameterName = "@MD5";
                cmdInsertDs.Parameters.Add(p);
            }
            else
            {
                // now logging schema uses string for easier usability and FAnsiSql compatibility
                DatabaseSettings.AddParameterWithValueToCommand("@MD5", cmdInsertDs,
                    s.MD5 != null ? s.MD5 : DBNull.Value);
            }

            s.ID = int.Parse(cmdInsertDs.ExecuteScalar().ToString());
        }
    }

    #region Property setup

    /// <summary>
    ///     Increases or Gets the number of Updated records, use += instead of trying to set a specific value.  Important:Make
    ///     sure you increment with Affected Rows, not just UPDATE commands sent)
    /// </summary>
    public int Updates { get; set; }


    /// <summary>
    ///     Increases or Gets the number of Deleted records, use += instead of trying to set a specific value.  Important:Make
    ///     sure you increment with Affected Rows, not just DELETE commands sent)
    /// </summary>
    public int Deletes { get; set; }

    /// <summary>
    ///     Increases or Gets the number of Inserted records, use += instead of trying to set a specific value
    /// </summary>
    public int Inserts { get; set; }

    /// <summary>
    ///     Increases or Gets the number of Discarded Duplicate records, use += instead of trying to set a specific value
    /// </summary>
    public int DiscardedDuplicates { get; set; }

    /// <summary>
    ///     Gets the number of ErrorRows during this data run so far, this is automatically increased by the RowErrorLogging
    ///     class
    /// </summary>
    public int ErrorRows => _errorRows;


    public string SuggestedRollbackCommand { get; }


    public int ID { get; private set; }

    public DateTime StartTime { get; }


    public DateTime EndTime { get; private set; }


    public string Notes { get; set; }

    public bool IsLegacyLoggingSchema { get; }

    public void CloseAndArchive()
    {
        using var con = DatabaseSettings.BeginNewTransactedConnection();
        using var cmdCloseRecord = DatabaseSettings.GetCommand(
            "UPDATE TableLoadRun SET endTime=@endTime,inserts=@inserts,updates=@updates,deletes=@deletes,errorRows=@errorRows,duplicates=@duplicates, notes=@notes WHERE ID=@ID",
            con.Connection, con.ManagedTransaction);
        try
        {
            DatabaseSettings.AddParameterWithValueToCommand("@endTime", cmdCloseRecord, DateTime.Now);
            DatabaseSettings.AddParameterWithValueToCommand("@inserts", cmdCloseRecord, Inserts);
            DatabaseSettings.AddParameterWithValueToCommand("@updates", cmdCloseRecord, Updates);
            DatabaseSettings.AddParameterWithValueToCommand("@deletes", cmdCloseRecord, Deletes);
            DatabaseSettings.AddParameterWithValueToCommand("@errorRows", cmdCloseRecord, ErrorRows);
            DatabaseSettings.AddParameterWithValueToCommand("@duplicates", cmdCloseRecord, DiscardedDuplicates);
            DatabaseSettings.AddParameterWithValueToCommand("@notes", cmdCloseRecord,
                string.IsNullOrWhiteSpace(Notes) ? DBNull.Value : Notes);
            DatabaseSettings.AddParameterWithValueToCommand("@ID", cmdCloseRecord, ID);

            var affectedRows = cmdCloseRecord.ExecuteNonQuery();

            if (affectedRows != 1)
                throw new Exception(
                    $"Error closing TableLoadInfo in database, the UPDATE command affected {affectedRows} when we expected 1 (will attempt to rollback transaction)");

            foreach (var s in DataSources)
                MarkDataSourceAsArchived(s, con);

            con.ManagedTransaction.CommitAndCloseConnection();

            EndTime = DateTime.Now;
            IsClosed = true;
        }
        catch (Exception)
        {
            //if something goes wrong with the update, roll it back
            con.ManagedTransaction.AbandonAndCloseConnection();
            throw;
        }
    }

    private void MarkDataSourceAsArchived(DataSource ds, IManagedConnection con)
    {
        if (string.IsNullOrEmpty(ds.Archive))
            return;

        using var cmdSetArchived = DatabaseSettings.GetCommand(
            "UPDATE DataSource SET archive=@archive, source = @source WHERE ID=@ID", con.Connection,
            con.ManagedTransaction);
        DatabaseSettings.AddParameterWithValueToCommand("@archive", cmdSetArchived, ds.Archive);
        DatabaseSettings.AddParameterWithValueToCommand("@source", cmdSetArchived, ds.Source);
        DatabaseSettings.AddParameterWithValueToCommand("@ID", cmdSetArchived, ds.ID);

        cmdSetArchived.ExecuteNonQuery();
    }

    public void IncrementErrorRows()
    {
        //ensure thread safety
        Interlocked.Increment(ref _errorRows);
    }

    #endregion
}