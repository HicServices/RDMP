using FAnsi.Discovery;
using NPOI.OpenXmlFormats;
using Rdmp.Core.Curation.Data;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

//todo list
// use the mapping
//make the ui nice
//make sure the checks work
//write some tests


public class ExecuteCommandUpdateCatalogueDataLocation : BasicCommandExecution, IAtomicCommand
{
    private IBasicActivateItems _activator;
    private readonly DiscoveredTable _table; 
    private readonly CatalogueItem[] _selectedCatalogueItems;
    private readonly string _catalogueMapping;
    
    private bool _checksPassed = false;
    public ExecuteCommandUpdateCatalogueDataLocation(IBasicActivateItems activator, CatalogueItem[] selectedCatalogueItems, DiscoveredTable table, string catalogueMapping)
    {
        _activator = activator;
        _table = table;
        _selectedCatalogueItems = selectedCatalogueItems;
        _catalogueMapping = catalogueMapping;
    }

    public bool Check()
    {
        //todo check the columns actually exist & that the types match && that we can talk to the db
        _checksPassed = true;
        return true;
    }

    private string GrabTablequalifier(string name)
    {
        return string.Join('.', name.Split('.')[..^1]);
    }


    private string GenerateNewSQLPath(string path)
    {
        //todo use the mapping 

        var qualifier = GrabTablequalifier(path);
        var updatedName = path.Replace(qualifier,_table.GetFullyQualifiedName());
        return updatedName;
    }

    private TableInfo TableIsAlreadyKnown()
    {
        return _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>().Where(ti => {
            return ti.Name == _table.GetFullyQualifiedName() &&
            ti.Server == _table.Database.Server.Name &&
            ti.Database == _table.Database.GetRuntimeName();
            }).FirstOrDefault();
    }


    public override void Execute()
    {
        if(!_checksPassed)
        {
            throw new Exception("Unable to execute ExecuteCommandUpdateCatalogueDataLocation as checks have not been ran.");
        }
        

        foreach(CatalogueItem selectedCatalogueItem in _selectedCatalogueItems)
        {
            var existingTable = TableIsAlreadyKnown();
            if (existingTable is not null)
            {
                selectedCatalogueItem.ColumnInfo.TableInfo_ID = existingTable.ID;
            }
            else
            {
                var tblInfo = new TableInfo(_activator.RepositoryLocator.CatalogueRepository,_table.GetFullyQualifiedName());
                tblInfo.Server = _table.Database.Server.Name;
                tblInfo.Database= _table.Database.GetRuntimeName();
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
