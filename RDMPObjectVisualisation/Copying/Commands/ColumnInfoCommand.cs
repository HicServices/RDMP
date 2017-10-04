using System;
using System.Linq;
using CatalogueLibrary.Data;
using ReusableUIComponents.Copying;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class ColumnInfoCommand : ICommand
    {
        public ColumnInfo[] ColumnInfos { get; private set; }

        public ColumnInfoCommand(ColumnInfo columnInfo)
        {
            ColumnInfos = new []{columnInfo};
        }

        public ColumnInfoCommand(ColumnInfo[] columnInfos)
        {
            ColumnInfos = columnInfos;
        }

        public string GetSqlString()
        {
            return string.Join(Environment.NewLine,ColumnInfos.Select(c=>c.Name));
        }
    }
}