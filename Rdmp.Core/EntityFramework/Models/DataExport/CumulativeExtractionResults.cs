using CsvHelper.Configuration.Attributes;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    public class CumulativeExtractionResults: DatabaseObject, ICumulativeExtractionResults
    {

        [Key]
        public override int ID { get; set; }

        public int ExtractionConfiguration_ID { get; set; }
        public int ExtractableDataSet_ID { get; set; }
        public int DistinctReleaseIdentifiersEncountered { get ; set ; }
        public string FiltersUsed { get ; set ; }

        public int CohortExtracted { get; set; }

        [ForeignKey("ExtractableDataSet_ID")]
        public virtual ExtractableDataSet ExtractableDataSet { get; set; }

        [ForeignKey("ExtractionConfiguration_ID")]
        public virtual ExtractionConfiguration ExtractionConfiguration { get; set; }

        [NotMapped]
        public List<ISupplementalExtractionResults> SupplementalExtractionResults => throw new NotImplementedException();


        public string DestinationDescription { get; set; }

        public string DestinationType { get; set; }

        public int RecordsExtracted { get; set; }

        public DateTime DateOfExtraction { get; set; }

        public string Exception { get; set; }

        public string SQLExecuted { get; set; }

        [NotMapped]
        IExtractableDataSet ICumulativeExtractionResults.ExtractableDataSet => throw new NotImplementedException();

        public IReleaseLog GetReleaseLogEntryIfAny()
        {
            throw new NotImplementedException();
        }

        public ISupplementalExtractionResults AddSupplementalExtractionResult(string sqlExecuted, IMapsDirectlyToDatabaseTable extractedObject)
        {
            throw new NotImplementedException();
        }

        public bool IsFor(ISelectedDataSets selectedDataSet)
        {
            throw new NotImplementedException();
        }

        public Type GetDestinationType()
        {
            throw new NotImplementedException();
        }

        public void CompleteAudit(Type destinationType, string destinationDescription, int recordsExtracted, bool isBatchResume, bool failed)
        {
            throw new NotImplementedException();
        }

        public bool IsReferenceTo(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsReferenceTo(IMapsDirectlyToDatabaseTable o)
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }
    }
}
