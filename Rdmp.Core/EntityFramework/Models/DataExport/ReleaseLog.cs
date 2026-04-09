using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("ReleaseLog")]
    public class ReleaseLog: DatabaseObject, IReleaseLog
    {
        [Key]
        public override int ID { get; set; }

        public string Username { get; set; }
        public DateTime DateOfRelease { get; set; }
        public int CumulativeExtractionResults_ID { get; set; }
        public string MD5OfDatasetFile { get; set; }
        public string DatasetState { get; set; }
        public string EnvironmentState { get; set; }
        public bool IsPatch { get; set; }
        public string ReleaseFolder { get; set; }

        [ForeignKey("CumulativeExtractionResults_ID")]
        public virtual CumulativeExtractionResults CumulativeExtractionResults { get; set; }

        public override string ToString()
        {
            string _datasetName = CumulativeExtractionResults.ExtractableDataSet.ToString();
            return
                $"ReleaseLogEntry(CumulativeExtractionResults_ID={CumulativeExtractionResults_ID},DatasetName={_datasetName},DateOfRelease={DateOfRelease},Username={Username})";
        }
    }
}
