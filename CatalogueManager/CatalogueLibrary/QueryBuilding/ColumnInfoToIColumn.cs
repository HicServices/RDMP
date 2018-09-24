using System;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// Allows you to convert a ColumnInfo into an IColumn (the column concept in query building).  IColumn has Alias and Order which do not exist in ColumnInfo 
    /// (which is a reference to an existing column on your database only).  The alias will be null and the Order will be -1 meaning that ColumnInfoToIColumn will
    /// by default appear above other IColumns in order.
    /// </summary>
    public class ColumnInfoToIColumn :SpontaneousObject, IColumn
    {
        static Random r = new Random();

        /// <summary>
        /// Allows the given <see cref="ColumnInfo"/> to act as an <see cref="IColumn"/> giving it an Order and setting extraction flags (e.g. <see cref="HashOnDataRelease"/>)to sensible defaults.
        /// </summary>
        /// <param name="column"></param>
        public ColumnInfoToIColumn(ColumnInfo column)
        {
            ColumnInfo = column;
            Order = -1;
            SelectSQL = column.GetRuntimeName();
            Alias = null;
            HashOnDataRelease = false;
            IsExtractionIdentifier = false;
            IsPrimaryKey = false;
        }
        
        /// <inheritdoc/>
        public string GetRuntimeName()
        {
            return ColumnInfo.GetRuntimeName();
        }
        
        /// <inheritdoc/>
        public ColumnInfo ColumnInfo { get; private set; }

        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc/>
        [Sql]
        public string SelectSQL { get; set; }
        
        /// <inheritdoc/>
        public string Alias { get; set; }
        /// <inheritdoc/>
        public bool HashOnDataRelease { get; private set; }
        /// <inheritdoc/>
        public bool IsExtractionIdentifier { get; private set; }
        /// <inheritdoc/>
        public bool IsPrimaryKey { get; private set; }
        
        /// <summary>
        /// Checks the syntax of the column
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }
}