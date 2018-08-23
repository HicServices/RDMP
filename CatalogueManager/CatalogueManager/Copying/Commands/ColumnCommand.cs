using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class ColumnCommand : ICommand
    {
        public readonly IColumn Column;

        public ColumnCommand(IColumn column)
        {
            Column = column;
        }

        public string GetSqlString()
        {
            return Column.SelectSQL;
        }
    }
}