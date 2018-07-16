using System;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Spontaneous
{
    public class SpontaneousColumn : IColumn
    {
        public string GetRuntimeName()
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public ColumnInfo ColumnInfo { get; private set; }
        public int Order { get; set; }
        public string SelectSQL { get; set; }
        public int ID { get; set; }
        public string Alias { get; set; }
        public bool HashOnDataRelease { get; set; }
        public bool IsExtractionIdentifier { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}