using System;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) implementation of IColumn.
    /// </summary>
    public class SpontaneouslyInventedColumn : SpontaneousObject,IColumn
    {
        public SpontaneouslyInventedColumn(string alias, string selectSQl)
        {
            ColumnInfo = null;//no underlying column
            if(string.IsNullOrWhiteSpace(alias))
                throw new Exception("Column must have an alias");
            Alias = alias;
            SelectSQL = selectSQl;

        }
        public string GetRuntimeName()
        {
            return Alias;
        }

        public void Check(ICheckNotifier notifier)
        {
            
        }
        public string SelectSQL { get; set; }
        public string Alias { get; private set; }

        public ColumnInfo ColumnInfo { get; private set; }
        public int Order { get; set; }
        public bool HashOnDataRelease { get; set; }
        public bool IsExtractionIdentifier { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}