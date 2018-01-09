using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.DataRelease.Audit
{
    /// <summary>
    /// Records the fact that a given dataset has been succesfully extracted (See ReleaseLogWriter)
    /// 
    /// Cannot be updated (saved) and does not have it's own ID - there is a 1 to 1 relationship between cumulative extraction results and ReleaseLog
    /// </summary>
    public class ReleaseLogEntry : IReleaseLogEntry
    {
        private readonly IRepository _repository;
        public int CumulativeExtractionResults_ID { get; private set; }
        public string Username { get; private set; }
        public DateTime DateOfRelease { get; private set; }
        public string MD5OfDatasetFile { get; private set; }
        public string DatasetState { get; private set; }
        public string EnvironmentState { get; private set; }
        public bool IsPatch { get; private set; }
        public string ReleaseFolder { get; private set; }

        private string _datasetName;
        public override string ToString()
        {
            if (_datasetName == null)
                try
                {
                    var cumulativeExtractionResults = _repository.GetObjectByID<CumulativeExtractionResults>(CumulativeExtractionResults_ID);
                    ExtractableDataSet ds = _repository.GetObjectByID<ExtractableDataSet>(cumulativeExtractionResults.ExtractableDataSet_ID);
                    _datasetName = ds.ToString();
                }
                catch (Exception e)
                {
                    _datasetName = e.Message;
                }


            return
                string.Format(
                    "ReleaseLogEntry(CumulativeExtractionResults_ID={0},DatasetName={1},DateOfRelease={2},Username={3})",
                    CumulativeExtractionResults_ID,
                    _datasetName,
                    DateOfRelease,
                    Username);
        }

        public ReleaseLogEntry(IRepository repository, DbDataReader r)
        {
            _repository = repository;
            CumulativeExtractionResults_ID = Convert.ToInt32(r["CumulativeExtractionResults_ID"]);
            Username = r["Username"].ToString();
            MD5OfDatasetFile = r["MD5OfDatasetFile"].ToString();
            DatasetState = r["DatasetState"].ToString();
            EnvironmentState = r["EnvironmentState"].ToString();
            DateOfRelease = Convert.ToDateTime(r["DateOfRelease"]);
            IsPatch = Convert.ToBoolean(r["IsPatch"]);
            ReleaseFolder = r["ReleaseFolder"].ToString();
        }

        public void DeleteInDatabase()
        {
            var affectedRows = _repository.Delete(
                @"DELETE FROM [ReleaseLog] where CumulativeExtractionResults_ID = @CumulativeExtractionResults_ID",
                new Dictionary<string, object>
                {
                    {"CumulativeExtractionResults_ID", CumulativeExtractionResults_ID}
                });

            if(affectedRows != 1)
                throw new Exception("Attempted to delete a ReleaseLog entry (CumulativeExtractionResults_ID=" + CumulativeExtractionResults_ID + ") but result was " + affectedRows + " affected rows (expected 1)");
        }
    }
}