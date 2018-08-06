using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class TableInfoCommand : ICommand
    {
        public TableInfo TableInfo { get; private set; }

        public TableInfoCommand(TableInfo tableInfo)
        {
            TableInfo = tableInfo;
        }

        public string GetSqlString()
        {
            return TableInfo.Name;
        }
    }
}