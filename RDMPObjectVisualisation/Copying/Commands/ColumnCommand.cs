using CatalogueLibrary.Data;
using ReusableUIComponents.Copying;

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