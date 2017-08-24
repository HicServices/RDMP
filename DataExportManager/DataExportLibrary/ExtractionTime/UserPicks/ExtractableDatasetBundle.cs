using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.ExtractionTime.UserPicks
{
    public class ExtractableDatasetBundle : Bundle, IExtractableDatasetBundle
    {
        //The main dataset being extracted
        public IExtractableDataSet DataSet { get; private set; }

        //all the rest of the stuff that goes with the dataset
        public List<SupportingDocument> Documents { get; private set; }
        public List<SupportingSQLTable> SupportingSQL { get; private set; }
        public List<IBundledLookupTable> LookupTables { get; private set; }
        

        public ExtractableDatasetBundle(IExtractableDataSet dataSet, SupportingDocument[] documents, SupportingSQLTable[] supportingSQL, TableInfo[] lookupTables) : 
            base(
                new [] {(object)dataSet}.Union(documents).Union(supportingSQL).Union(lookupTables).ToArray() //pass all the objects to the base class so it can allocate initial States
            )
        {
            DataSet = dataSet;
            Documents = documents.ToList();
            SupportingSQL = supportingSQL.ToList();
            LookupTables = lookupTables.Select(t => new BundledLookupTable(t)).Cast<IBundledLookupTable>().ToList();
        }

        public ExtractableDatasetBundle(IExtractableDataSet dataSet)
            : this(dataSet, new SupportingDocument[0], new SupportingSQLTable[0], new TableInfo[0])
        {
        }
        
        public override string ToString()
        {
            return DataSet + " Bundle";
        }

        protected override void OnDropContent(object toDrop)
        {
            if(toDrop is ExtractableDataSet)
                throw new NotSupportedException("Cannot drop "+toDrop+" from Bundle "+this+", you cannot perform an extraction without the dataset component (only documents/lookups etc are optional)");

            var drop = toDrop as SupportingDocument;
            if (drop != null)
            {
                Documents.Remove(drop);
                return;
            }

            var item = toDrop as SupportingSQLTable;
            if (item != null)
            {
                SupportingSQL.Remove(item);
                return;
            }

            var table = toDrop as BundledLookupTable;
            if (table != null)
            {
                LookupTables.Remove(table);
                return;
            }

            throw new NotSupportedException("Did not know how to drop object of type " + toDrop);
        }
    }
}
