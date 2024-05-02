// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Sources;

/// <inheritdoc />
public class DbDataCommandDataFlowSource : IDbDataCommandDataFlowSource
{
    public string Sql { get; }
    private DbDataReader _reader;
    private readonly DbConnectionStringBuilder _builder;
    private readonly int _timeout;
    private DbConnection _con;

    private readonly string _taskBeingPerformed;
    private readonly Stopwatch timer = new();

    public int BatchSize { get; set; }

    public DbCommand cmd { get; private set; }

    public bool AllowEmptyResultSets { get; set; }
    public int TotalRowsRead { get; set; }

    /// <summary>
    ///     Called after command sql has been set up, allows last minute changes by subscribers before it is executed
    /// </summary>
    public Action<DbCommand> CommandAdjuster { get; set; }

    public DbDataCommandDataFlowSource(string sql, string taskBeingPerformed, DbConnectionStringBuilder builder,
        int timeout)
    {
        Sql = sql;
        _taskBeingPerformed = taskBeingPerformed;
        _builder = builder;
        _timeout = timeout;

        BatchSize = 10000;
    }

    private int _numberOfColumns;

    private bool _firstChunk = true;

    private DataTable schema;

    public DataTable GetChunk(IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
    {
        if (_reader == null)
        {
            _con = DatabaseCommandHelper.GetConnection(_builder);
            _con.Open();

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Running SQL:{Environment.NewLine}{Sql}"));

            cmd = DatabaseCommandHelper.GetCommand(Sql, _con);
            cmd.CommandTimeout = _timeout;
            CommandAdjuster?.Invoke(cmd);

            _reader = cmd.ExecuteReaderAsync(cancellationToken.AbortToken).Result;
            _numberOfColumns = _reader.FieldCount;

            schema = GetChunkSchema(_reader);
        }

        var readThisBatch = 0;
        timer.Start();
        try
        {
            var chunk = schema.Clone();
            chunk.BeginLoadData();

            while (_reader.HasRows && _reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddRowToDataTable(chunk, _reader);
                readThisBatch++;

                // loop until we reach the batch limit
                if (readThisBatch != BatchSize) continue;

                chunk.EndLoadData();
                return chunk;
            }

            chunk.EndLoadData();

            //if data was read
            if (readThisBatch > 0)
            {
                chunk.EndLoadData();
                return chunk;
            }

            //data is exhausted

            //if data was exhausted on first read and we are allowing empty result sets
            if (_firstChunk && AllowEmptyResultSets)
            {
                chunk.EndLoadData();
                return chunk; //return the empty chunk
            }

            //data exhausted
            schema.Dispose();
            return null;
        }
        catch (Exception e)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Source read failed", e));
            throw;
        }
        finally
        {
            _firstChunk = false;
            timer.Stop();
            job.OnProgress(this,
                new ProgressEventArgs(_taskBeingPerformed, new ProgressMeasurement(TotalRowsRead, ProgressType.Records),
                    timer.Elapsed));
        }
    }

    private DataRow AddRowToDataTable(DataTable chunk, DbDataReader reader)
    {
        var values = new object[_numberOfColumns];

        reader.GetValues(values);
        TotalRowsRead++;
        return chunk.LoadDataRow(values, LoadOption.Upsert);
    }

    /// <inheritdoc />
    public DataRow ReadOneRow()
    {
        //return null if there are no more records to read
        return _reader.Read() ? AddRowToDataTable(GetChunkSchema(_reader), _reader) : null;
    }

    private static DataTable GetChunkSchema(DbDataReader reader)
    {
        var toReturn = new DataTable("dt");

        //Retrieve column schema into a DataTable.
        var schemaTable = reader.GetSchemaTable() ??
                          throw new InvalidOperationException(
                              "Could not retrieve schema information from the DbDataReader");
        Debug.Assert(schemaTable.Columns[0].ColumnName.ToLower().Contains("name"));

        //For each field in the table...
        foreach (DataRow myField in schemaTable.Rows)
        {
            var t = Type.GetType(myField["DataType"].ToString()) ??
                    throw new NotSupportedException($"Type.GetType failed on SQL DataType:{myField["DataType"]}");

            //let's not mess around with floats, make everything a double please
            if (t == typeof(float))
                t = typeof(double);


            toReturn.Columns.Add(myField[0].ToString(), t); //0 should always be the column name
        }

        return toReturn;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        CloseReader(listener);
    }

    public void Abort(IDataLoadEventListener listener)
    {
        CloseReader(listener);
    }

    private void CloseReader(IDataLoadEventListener listener)
    {
        try
        {
            if (_con == null)
                return;

            if (_con.State != ConnectionState.Closed)
                _con.Close();

            _reader?.Dispose();
            cmd?.Dispose();

            //do not do this more than once! which could happen if they abort then it disposes
            _con = null;
        }
        catch (Exception e)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning, "Could not close Reader / Connection", e));
        }
    }

    public DataTable TryGetPreview()
    {
        var chunk = new DataTable();
        using var con = DatabaseCommandHelper.GetConnection(_builder);
        con.Open();
        using var da = DatabaseCommandHelper.GetDataAdapter(DatabaseCommandHelper.GetCommand(Sql, con));
        var read = da.Fill(0, 100, chunk);

        return read == 0 ? null : chunk;
    }
}