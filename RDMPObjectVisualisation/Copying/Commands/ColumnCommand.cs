using CatalogueLibrary.Data;
using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class ColumnCommand : ICommand
    {
        private readonly IColumn _c;

        public ColumnCommand(IColumn c)
        {
            _c = c;
        }

        public string GetSqlString()
        {
            return _c.SelectSQL;
        }
    }
}