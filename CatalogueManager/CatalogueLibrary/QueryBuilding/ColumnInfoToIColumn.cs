using System;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// Allows you to convert a ColumnInfo into an IColumn (the column concept in query building).  IColumn has Alias and Order which do not exist in ColumnInfo 
    /// (which is a reference to an existing column on your database only).  The alias will be null and the Order will be -1 meaning that ColumnInfoToIColumn will
    /// by default appear above other IColumns in order.
    /// </summary>
    public class ColumnInfoToIColumn : IColumn
    {
        static Random r = new Random();

        public ColumnInfoToIColumn(ColumnInfo column)
        {
            ColumnInfo = column;
            Order = -1;
            SelectSQL = column.GetRuntimeName();
            ID = -1 * r.Next(1,5000000);
            Alias = null;
            HashOnDataRelease = false;
            IsExtractionIdentifier = false;
            IsPrimaryKey = false;
        }

        public string GetRuntimeName()
        {
            return ColumnInfo.GetRuntimeName();
        }

        public ColumnInfo ColumnInfo { get; private set; }
        public int Order { get; set; }
        public string SelectSQL { get; set; }
        public int ID { get; private set; }
        public string Alias { get; set; }
        public bool HashOnDataRelease { get; private set; }
        public bool IsExtractionIdentifier { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }
}