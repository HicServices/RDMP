// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.DataFlowPipeline;
using Rdmp.Core.CatalogueLibrary.QueryBuilding;
using Rdmp.Core.CatalogueLibrary.Spontaneous;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping
{
    /// <summary>
    /// Swaps values stored in a given column for values found in a mapping table (e.g. swap ReleaseID for PrivateID)
    /// </summary>
    class ColumnSwapper:IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("The column in your database which stores the input values you want mapped", Mandatory = true)]
        public ColumnInfo MappingFromColumn { get; set; }

        [DemandsInitialization("The column in your database which stores the output values you want emitted", Mandatory = true)]
        public ColumnInfo MappingToColumn { get; set; }

        [DemandsInitialization("Optional text to add when generating the mapping table. Should not start with WHERE")]
        public string WHERELogic { get; set; }

        [DemandsInitialization("Determines behaviour when the same input value maps to multiple output values", DefaultValue = AliasResolutionStrategy.CrashIfAliasesFound)]
        public AliasResolutionStrategy AliasResolutionStrategy { get; set; }

        [DemandsInitialization(@"Determines behaviour when no mapping is found for an input value:
True - Crash the load
False - Drop the row from the DataTable (and issue a warning)",DefaultValue=true)]
        public bool CrashIfNoMappingsFound { get; set; }

        [DemandsInitialization("Timeout to set on fetching the mapping table",DefaultValue=30)]
        public int Timeout { get; set; }

        [DemandsInitialization(@"Setting this to true will leave the original input column in your DataTable (so your table will have both input and output columns instead of a substitution)", DefaultValue = true)]
        public bool KeepInputColumnToo { get; set; }

        Dictionary<object,List<object>> _mappingTable;

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            var fromColumnName = MappingFromColumn.GetRuntimeName();
            var toColumnName = MappingToColumn.GetRuntimeName();

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to build mapping table"));

            if(!toProcess.Columns.Contains(fromColumnName))
                throw new Exception("DataTable did not contain a field called '" + fromColumnName +"'");

            if (toProcess.Columns.Contains(toColumnName))
                throw new Exception("DataTable already contained a field '" + toColumnName +"'");

            if(_mappingTable == null)
                BuildMappingTable();

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Mapping table resulted in " + _mappingTable.Count + " unique possible input values"));

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Mapping table resulted in " + _mappingTable.Sum(kvp=>kvp.Value.Count) + " unique possible output values"));

            //add the new column (the output column)
            toProcess.Columns.Add(toColumnName);

            int idxFrom = toProcess.Columns.IndexOf(fromColumnName);
            int idxTo = toProcess.Columns.IndexOf(toColumnName);

            int numberOfElementsPerRow = toProcess.Columns.Count;

            List<object[]> newRows = new List<object[]>();
            List<DataRow> toDrop = new List<DataRow>();
            
            foreach (DataRow row in toProcess.Rows)
            {
                var fromValue = row[idxFrom];
                
                //if we don't have the key value
                if(!_mappingTable.ContainsKey(fromValue))
                    if(CrashIfNoMappingsFound)
                        throw new KeyNotFoundException("Could not find mapping for " + fromValue);
                    else
                    {
                        listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "No mapping for '" + fromValue+"' dropping row"));
                        toDrop.Add(row);
                        continue;
                    }
                
                //we do have the key value!
                var results = _mappingTable[fromValue];

                //yes 1
                if (results.Count == 1)
                    row[idxTo] = results.Single();
                else
                {
                    //great we have multiple mappings, bob=>Frank and bob=>Jesus.  What does the user want to do about that
                    switch (AliasResolutionStrategy)
                    {
                        case AliasResolutionStrategy.CrashIfAliasesFound:
                            throw new AliasException("The value '" + fromValue + "' maps to mulitple ouptut values:" + string.Join(",",results.Select(v=>"'" + v.ToString() +"'")) );

                        case AliasResolutionStrategy.MultiplyInputDataRowsByAliases:
                            
                            //substitute for the first alias (bob=>Frank)
                            row[idxTo] = results.First();

                            //then clone the row and do a row with bob=>Jesus
                            foreach (object next in results.Skip(1))
                            {
                                //Create a copy of the input row
                                object[] newRow = new object[numberOfElementsPerRow];
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
            }
            
            //add any alias multiplication rows
            foreach (object[] newRow in newRows)
                toProcess.Rows.Add(newRow);

            //drop rows with missing identifiers
            foreach (DataRow dropRow in toDrop)
                toProcess.Rows.Remove(dropRow);

            if(!KeepInputColumnToo)
                toProcess.Columns.Remove(fromColumnName);
            
            return toProcess;
        }

        private void BuildMappingTable()
        {
            //Get a new mapping table in memory
            _mappingTable = new Dictionary<object, List<object>>();

            //connect to server and run distinct query
            var server = MappingFromColumn.TableInfo.Discover(DataAccessContext.DataLoad).Database.Server;

            var fromColumnName = MappingFromColumn.GetRuntimeName();
            var toColumnName = MappingToColumn.GetRuntimeName();

            //pull back all the data
            using (var con = server.GetConnection())
            {
                con.Open();
                var sql = GetMappingTableSql();
                var cmd = server.GetCommand(sql, con);
                cmd.CommandTimeout = Timeout;

                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    if(!_mappingTable.ContainsKey(r[fromColumnName]))
                        _mappingTable.Add(r[fromColumnName],new List<object>());

                    _mappingTable[r[fromColumnName]].Add(r[toColumnName]);
                }
            }
        }

        private string GetMappingTableSql()
        {
            var repo = new MemoryCatalogueRepository();

            var qb = new QueryBuilder("DISTINCT", null, null);
            qb.AddColumn(new ColumnInfoToIColumn(repo,MappingFromColumn));
            qb.AddColumn(new ColumnInfoToIColumn(repo,MappingToColumn));

            if (!string.IsNullOrWhiteSpace(WHERELogic))
            {
                var container = new SpontaneouslyInventedFilterContainer(repo,null, null, FilterContainerOperation.AND);
                var filter = new SpontaneouslyInventedFilter(repo,container, WHERELogic, "WHERELogic", null, null);
                container.AddChild(filter);

                qb.RootFilterContainer = container;
            }

            return qb.SQL;
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

        public void Check(ICheckNotifier notifier)
        {
            if(!string.IsNullOrWhiteSpace(WHERELogic))
                if(WHERELogic.StartsWith("WHERE"))
                    throw new Exception("WHERE logic should not start with WHERE");

            if(MappingFromColumn == null || MappingToColumn == null)
                throw new Exception("Mapping From/To Column missing, these are Mandatory");

            if(MappingFromColumn.TableInfo_ID != MappingToColumn.TableInfo_ID)
                throw new Exception("MappingFromColumn and MappingToColumn must belong to the same table");

            notifier.OnCheckPerformed(new CheckEventArgs("Mapping table SQL is:"+ Environment.NewLine + GetMappingTableSql(),CheckResult.Success));

        }
    }
}
