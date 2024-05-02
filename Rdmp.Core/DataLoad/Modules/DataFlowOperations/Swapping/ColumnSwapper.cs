// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping;

/// <summary>
///     Swaps values stored in a given column for values found in a mapping table (e.g. swap ReleaseID for PrivateID)
/// </summary>
internal class ColumnSwapper : IPluginDataFlowComponent<DataTable>, IPipelineOptionalRequirement<IExtractCommand>,
    IPipelineOptionalRequirement<ICohortCreationRequest>
{
    [DemandsInitialization(
        "The column in your pipeline containing input values you want swapped.  Leave null to use the same name as the MappingFromColumn")]
    public string InputFromColumn { get; set; }

    [DemandsInitialization(
        "Name for the column you want to create in the output stream of this component containing the mapped values.  Leave null to use the same name as the MappingToColumn")]
    public string OutputToColumn { get; set; }

    [DemandsInitialization("The column in your database which stores the input values you want mapped",
        Mandatory = true)]
    public ColumnInfo MappingFromColumn { get; set; }

    [DemandsInitialization("The column in your database which stores the output values you want emitted",
        Mandatory = true)]
    public ColumnInfo MappingToColumn { get; set; }

    [DemandsInitialization(@"Optional text to add when generating the mapping table. Should not start with WHERE.

If Pipeline execution environment contains a Project then the following replacements are available:
    $p - Project Name ('e.g. My Project')
    $n - Project Number (e.g. 234)
    $t - Master Ticket (e.g. 'LINK-1234')

If Pipeline execution environment contains an ExtractionConfiguration then the following additional replacements are available:
    $r - Request Ticket (e.g. 'LINK-1234')
    $l - Release Ticket (e.g. 'LINK-1234')", DemandType = DemandType.SQL, ContextText = "WHERE")]
    public virtual string WHERELogic { get; set; }

    [DemandsInitialization("Determines behaviour when the same input value maps to multiple output values",
        DefaultValue = AliasResolutionStrategy.CrashIfAliasesFound)]
    public AliasResolutionStrategy AliasResolutionStrategy { get; set; }

    [DemandsInitialization(@"Determines behaviour when no mapping is found for an input value:
True - Crash the load
False - Drop the row from the DataTable (and issue a warning)", DefaultValue = true)]
    public bool CrashIfNoMappingsFound { get; set; }

    [DemandsInitialization("Timeout to set on fetching the mapping table", DefaultValue = 30)]
    public int Timeout { get; set; }

    [DemandsInitialization(
        @"Setting this to true will leave the original input column in your DataTable (so your table will have both input and output columns instead of a substitution)",
        DefaultValue = true)]
    public bool KeepInputColumnToo { get; set; }

    private CultureInfo _culture;

    [DemandsInitialization("The culture to use e.g. when Type translations are required")]
    public CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    private Dictionary<object, List<object>> _mappingTable;

    /// <summary>
    ///     The Type of objects that are stored in the Keys of <see cref="_mappingTable" />.  For use when input types do not
    ///     match the mapping table types
    /// </summary>
    private Type _keyType;


    protected IProject _project;
    protected IExtractionConfiguration _configuration;

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var fromColumnName = string.IsNullOrWhiteSpace(InputFromColumn)
            ? MappingFromColumn.GetRuntimeName()
            : InputFromColumn;
        var toColumnName = string.IsNullOrWhiteSpace(OutputToColumn)
            ? MappingToColumn.GetRuntimeName()
            : OutputToColumn;

        var inPlace = string.Equals(fromColumnName, toColumnName);

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to build mapping table"));

        if (!toProcess.Columns.Contains(fromColumnName))
            throw new Exception($"DataTable did not contain a field called '{fromColumnName}'");

        if (!inPlace && toProcess.Columns.Contains(toColumnName))
            throw new Exception($"DataTable already contained a field '{toColumnName}'");

        if (_mappingTable == null)
            BuildMappingTable(listener);

        if (!_mappingTable.Any())
            throw new Exception("Mapping table was empty");

        if (_keyType == null)
            throw new Exception("Unable to determine key datatype for mapping table");

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Mapping table resulted in {_mappingTable.Count} unique possible input values"));
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Mapping table resulted in {_mappingTable.Sum(kvp => kvp.Value.Count)} unique possible output values"));
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Mapping table Key is of Type {_keyType}"));

        //add the new column (the output column).  Unless we are just updating the same input column
        if (!inPlace) toProcess.Columns.Add(toColumnName);


        var idxFrom = toProcess.Columns.IndexOf(fromColumnName);
        var idxTo = toProcess.Columns.IndexOf(toColumnName);

        var numberOfElementsPerRow = toProcess.Columns.Count;

        var newRows = new List<object[]>();
        var toDrop = new List<DataRow>();

        // Flag and anonymous method for converting between input data type and mapping table datatype
        var doTypeConversion = false;
        Func<object, object> typeConversion = null;

        //if there is a difference between the input column datatype and the mapping table datatatype
        if (toProcess.Columns[idxFrom].DataType != _keyType)
        {
            //tell the user
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    $"Input DataTable column {fromColumnName} is of data type {toProcess.Columns[idxFrom].DataType}, this differs from mapping table which is {_keyType}.  Type conversion will take place between these two Types when performing lookup"));
            doTypeConversion = true;

            //work out a suitable anonymous method for converting between the Types
            if (_keyType == typeof(string))
                typeConversion = a => a.ToString();
            else
                try
                {
                    var deciderFactory = new TypeDeciderFactory(Culture);
                    var decider = deciderFactory.Create(_keyType);
                    typeConversion = a => decider.Parse(a.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Error building Type conversion decider for the mapping table key type {_keyType}", ex);
                }
        }

        foreach (DataRow row in toProcess.Rows)
        {
            var fromValue = row[idxFrom];

            //ignore null inputs, pass them straight through
            if (fromValue == DBNull.Value)
            {
                row[idxTo] = DBNull.Value;
                continue;
            }

            //if we have to do a Type conversion
            if (doTypeConversion)
                // convert the input value to the mapping table key Type
                fromValue = typeConversion(fromValue);

            //if we don't have the key value
            if (!_mappingTable.ContainsKey(fromValue))
                if (CrashIfNoMappingsFound)
                {
                    throw new KeyNotFoundException($"Could not find mapping for {fromValue}");
                }
                else
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"No mapping for '{fromValue}' dropping row"));
                    toDrop.Add(row);
                    continue;
                }

            //we do have the key value!
            var results = _mappingTable[fromValue];

            //yes 1
            if (results.Count == 1)
                row[idxTo] = results.Single();
            else
                //great we have multiple mappings, bob=>Frank and bob=>Jesus.  What does the user want to do about that
                switch (AliasResolutionStrategy)
                {
                    case AliasResolutionStrategy.CrashIfAliasesFound:
                        throw new AliasException(
                            $"The value '{fromValue}' maps to multiple output values:{string.Join(",", results.Select(v => $"'{v}'"))}");

                    case AliasResolutionStrategy.MultiplyInputDataRowsByAliases:

                        //substitute for the first alias (bob=>Frank)
                        row[idxTo] = results.First();

                        //then clone the row and do a row with bob=>Jesus
                        foreach (var next in results.Skip(1))
                        {
                            //Create a copy of the input row
                            var newRow = new object[numberOfElementsPerRow];
                            row.ItemArray.CopyTo(newRow, 0);

                            //Set the aliasable element to the alias
                            newRow[idxTo] = next;

                            //Add it to our new rows collection
                            newRows.Add(newRow);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        //add any alias multiplication rows
        foreach (var newRow in newRows)
            toProcess.Rows.Add(newRow);

        //drop rows with missing identifiers
        foreach (var dropRow in toDrop)
            toProcess.Rows.Remove(dropRow);

        // drop column unless it is an inplace (no new columns) update or user wants to keep both
        if (!inPlace && !KeepInputColumnToo)
            toProcess.Columns.Remove(fromColumnName);

        return toProcess;
    }

    private void BuildMappingTable(IDataLoadEventListener listener)
    {
        //Get a new mapping table in memory
        _mappingTable = new Dictionary<object, List<object>>();

        //connect to server and run distinct query
        var server = MappingFromColumn.TableInfo.Discover(DataAccessContext.DataLoad).Database.Server;

        var fromColumnName = MappingFromColumn.GetRuntimeName();
        var toColumnName = MappingToColumn.GetRuntimeName();

        // The number of null key values found in the mapping table (these are ignored)
        var nulls = 0;

        //pull back all the data
        using (var con = server.GetConnection())
        {
            con.Open();
            var sql = GetMappingTableSql();

            using var cmd = server.GetCommand(sql, con);
            cmd.CommandTimeout = Timeout;

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var keyVal = r[fromColumnName];

                if (keyVal != DBNull.Value)
                {
                    if (_keyType == null)
                    {
                        _keyType = keyVal.GetType();
                    }
                    else
                    {
                        if (_keyType != keyVal.GetType())
                            throw new Exception(
                                $"Database mapping table Keys were of mixed Types {_keyType} and {keyVal.GetType()}");
                    }
                }
                else
                {
                    nulls++;
                    continue;
                }

                if (!_mappingTable.ContainsKey(keyVal))
                    _mappingTable.Add(keyVal, new List<object>());

                _mappingTable[keyVal].Add(r[toColumnName]);
            }
        }

        if (nulls > 0)
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    $"Discarded {nulls} Null key values read from mapping table"));
    }

    protected virtual string GetMappingTableSql()
    {
        var repo = new MemoryCatalogueRepository();

        var qb = new QueryBuilder("DISTINCT", null);
        qb.AddColumn(new ColumnInfoToIColumn(repo, MappingFromColumn));
        qb.AddColumn(new ColumnInfoToIColumn(repo, MappingToColumn));

        if (!string.IsNullOrWhiteSpace(WHERELogic))
        {
            var container = new SpontaneouslyInventedFilterContainer(repo, null, null, FilterContainerOperation.AND);
            var filter = new SpontaneouslyInventedFilter(repo, container, WHERELogic, "WHERELogic", null, null);
            container.AddChild(filter);

            qb.RootFilterContainer = container;
        }

        return AdjustForProjectTokens(qb.SQL);
    }

    private string AdjustForProjectTokens(string mappingTableSql)
    {
        if (mappingTableSql.Contains("$p"))
        {
            if (_project == null)
                throw new Exception("You cannot use $p in contexts where there is no Project available");

            mappingTableSql = mappingTableSql.Replace("$p",
                _project.Name ?? throw new Exception("Project didn't have a Project Name"));
        }

        if (mappingTableSql.Contains("$n"))
        {
            if (_project == null)
                throw new Exception("You cannot use $n in contexts where there is no Project available");

            mappingTableSql = mappingTableSql.Replace("$n",
                _project.ProjectNumber?.ToString() ??
                throw new Exception($"Project '{_project.Name}' didn't have a Project Number"));
        }

        if (mappingTableSql.Contains("$t"))
        {
            if (_project == null)
                throw new Exception("You cannot use $t in contexts where there is no Project available");

            mappingTableSql = mappingTableSql.Replace("$t",
                _project.MasterTicket ??
                throw new Exception($"Project '{_project.Name}' didn't have a Master Ticket"));
        }

        if (mappingTableSql.Contains("$r"))
        {
            if (_configuration == null)
                throw new Exception(
                    "You cannot use $r in contexts where there is no ExtractionConfiguration available");

            mappingTableSql = mappingTableSql.Replace("$r",
                _configuration.RequestTicket ??
                throw new Exception($"Extraction Configuration '{_configuration.Name}' didn't have a Request Ticket"));
        }

        if (mappingTableSql.Contains("$l"))
        {
            if (_configuration == null)
                throw new Exception(
                    "You cannot use $l in contexts where there is no ExtractionConfiguration available");

            mappingTableSql = mappingTableSql.Replace("$l",
                _configuration.ReleaseTicket ??
                throw new Exception($"Extraction Configuration '{_configuration.Name}' didn't have a Release Ticket"));
        }


        return mappingTableSql;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        //free up memory
        if (_mappingTable != null)
        {
            _mappingTable.Clear();
            _mappingTable = null;
        }
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public virtual void Check(ICheckNotifier notifier)
    {
        if (!string.IsNullOrWhiteSpace(WHERELogic))
            if (WHERELogic.StartsWith("WHERE"))
                throw new Exception("WHERE logic should not start with WHERE");

        if (MappingFromColumn == null || MappingToColumn == null)
            throw new Exception("Mapping From/To Column missing, these are Mandatory");

        if (MappingFromColumn.TableInfo_ID != MappingToColumn.TableInfo_ID)
            throw new Exception("MappingFromColumn and MappingToColumn must belong to the same table");

        notifier.OnCheckPerformed(new CheckEventArgs(
            $"Mapping table SQL is:{Environment.NewLine}{GetMappingTableSql()}", CheckResult.Success));
    }

    public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
    {
        _project = value.Configuration?.Project;
        _configuration = value.Configuration;
    }

    public void PreInitialize(ICohortCreationRequest value, IDataLoadEventListener listener)
    {
        _project = value.Project;
    }
}