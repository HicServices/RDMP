using CatalogueLibrary.Data;
using ReusableUIComponents.Copying;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class ColumnInfoCommand : ICommand
    {
        public ColumnInfo ColumnInfo { get; set; }

        public ColumnInfoCommand(ColumnInfo columnInfo)
        {
            ColumnInfo = columnInfo;
        }

        public string GetSqlString()
        {
            return ColumnInfo.Name;
        }
    }
}