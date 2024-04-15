// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

//todo list
//make the ui nice
//how does this work on the CLI?
//write some tests


public class ExecuteCommandUpdateCatalogueDataLocation : BasicCommandExecution, IAtomicCommand
{
    private IBasicActivateItems _activator;
    private readonly DiscoveredTable _table;
    private readonly CatalogueItem[] _selectedCatalogueItems;
    private readonly string _catalogueMapping;

    public readonly string CatalogueMappingIdentifier = "$column";

    private bool _checksPassed = false;
    public ExecuteCommandUpdateCatalogueDataLocation(IBasicActivateItems activator, CatalogueItem[] selectedCatalogueItems, DiscoveredTable table, string catalogueMapping)
    {
        _activator = activator;
        _table = table;
        _selectedCatalogueItems = selectedCatalogueItems;
        _catalogueMapping = catalogueMapping;
    }

    public string Check()
    {
        //check the server is alive
        _table.Database.Server.TestConnection();
        //must modify at least 
        if (_selectedCatalogueItems.Length == 0) return "Must select at least one catalogue item to modify";
        // check the mapping isn't junk
        if (!string.IsNullOrWhiteSpace(_catalogueMapping))
        {
            if (!_catalogueMapping.Contains(CatalogueMappingIdentifier)) return "Column Mapping must contain the string '$column'";
        }
        // check the columns actually exist & that the types match
        foreach (var item in _selectedCatalogueItems)
        {
            string newColumn;
            try
            {
                newColumn = GrabColumnName(item.ColumnInfo.Name);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            var discoveredColumns = _table.DiscoverColumns();
            var foundColumn = discoveredColumns.AsEnumerable().Where(dc => dc.GetFullyQualifiedName().Contains(newColumn)).FirstOrDefault();
            if (foundColumn is null)
            {
                return $"Unable to find column '{newColumn}' in selected table";
            }
            if (foundColumn.DataType.ToString() != item.ColumnInfo.Data_type) return $"The data type of column '{newColumn}' is of type '{foundColumn.DataType}'. This does not match the current type of '{item.ColumnInfo.Data_type}'";

        }
        _checksPassed = true;
        return null;
    }

    private string GrabTablequalifier(string name)
    {
        return string.Join('.', name.Split('.')[..^1]);
    }

    private string GrabColumnName(string name)
    {
        return MutilateColumnWithMapping(name.Split('.')[^1]);
    }

    private string MutilateColumnWithMapping(string columnName)
    {
        bool useParenthesis = false;
        if (columnName.StartsWith('[') && columnName.EndsWith("]"))
        {
            useParenthesis = true;
            columnName = columnName.Substring(1, columnName.Length - 2);
        }
        if (!string.IsNullOrWhiteSpace(_catalogueMapping))
        {
            if (!_catalogueMapping.Contains(CatalogueMappingIdentifier))
            {
                throw new Exception("Column Mapping is invalid. Add '$column' to the mapping string to fix this.");
            }
            else
            {
                columnName = _catalogueMapping.Replace(CatalogueMappingIdentifier, columnName);
                if (useParenthesis)
                {
                    columnName = '[' + columnName + ']';
                }
            }
        }
        return columnName;
    }


    private string GenerateNewSQLPath(string path)
    {
        var qualifier = GrabTablequalifier(path);
        var updatedName = qualifier + '.' + GrabColumnName(path);//path.Replace(qualifier,_table.GetFullyQualifiedName());
        return updatedName;
    }

    private TableInfo TableIsAlreadyKnown()
    {
        return _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>().Where(ti =>
        {
            return ti.Name == _table.GetFullyQualifiedName() &&
            ti.Server == _table.Database.Server.Name &&
            ti.Database == _table.Database.GetRuntimeName();
        }).FirstOrDefault();
    }


    public override void Execute()
    {
        if (!_checksPassed)
        {
            var checkResults = Check();
            if (checkResults != null)
            {
                throw new Exception($"Unable to execute ExecuteCommandUpdateCatalogueDataLocation as the checks returned: {checkResults}");
            }
        }

        foreach (CatalogueItem selectedCatalogueItem in _selectedCatalogueItems)
        {
            var existingTable = TableIsAlreadyKnown();
            if (existingTable is not null)
            {
                selectedCatalogueItem.ColumnInfo.TableInfo_ID = existingTable.ID;
            }
            else
            {
                var tblInfo = new TableInfo(_activator.RepositoryLocator.CatalogueRepository, _table.GetFullyQualifiedName());
                tblInfo.Server = _table.Database.Server.Name;
                tblInfo.Database = _table.Database.GetRuntimeName();
                tblInfo.SaveToDatabase();
                selectedCatalogueItem.ColumnInfo.TableInfo_ID = tblInfo.ID;

            }
            selectedCatalogueItem.ColumnInfo.Name = GenerateNewSQLPath(selectedCatalogueItem.ColumnInfo.Name);
            selectedCatalogueItem.ColumnInfo.SaveToDatabase();
            selectedCatalogueItem.ExtractionInformation.SelectSQL = GenerateNewSQLPath(selectedCatalogueItem.ExtractionInformation.SelectSQL);
            selectedCatalogueItem.ExtractionInformation.SaveToDatabase();
        }
    }
}
