using System;
using System.Collections.Generic;
using System.IO;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.DataRelease.Audit
{
    public class ReleaseLogWriter
    {
        private readonly ReleasePotential _dataset;
        private readonly ReleaseEnvironmentPotential _environment;
        private readonly IRepository _repository;
        
        public ReleaseLogWriter(ReleasePotential dataset, ReleaseEnvironmentPotential environment, IRepository repository)
        {
            _dataset = dataset;
            _environment = environment;
            _repository = repository;
        }

        public void GenerateLogEntry(bool isPatch, DirectoryInfo releaseDirectory, FileInfo datasetFileBeingReleased)
        {
            _repository.Insert("INSERT INTO ReleaseLog" +
                               @"([CumulativeExtractionResults_ID]
           ,[Username]
           ,[DateOfRelease]
           ,[MD5OfDatasetFile]
           ,[DatasetState]
           ,[EnvironmentState]
           ,[IsPatch]
           ,[ReleaseFolder])
VALUES
(@CumulativeExtractionResults_ID
           ,@Username
           ,@DateOfRelease
           ,@MD5OfDatasetFile
           ,@DatasetState
           ,@EnvironmentState
           ,@IsPatch
           ,@ReleaseFolder)", new Dictionary<string, object>
                               {
                                   {"CumulativeExtractionResults_ID", _dataset.ExtractionResults.ID},
                                   {"Username", Environment.UserName},
                                   {"DateOfRelease", DateTime.Now},
                                   {"MD5OfDatasetFile", UsefulStuff.MD5File(datasetFileBeingReleased.FullName)},
                                   {"DatasetState", _dataset.Assesment.ToString()},
                                   {"EnvironmentState", _environment.Assesment.ToString()},
                                   {"IsPatch", isPatch},
                                   {"ReleaseFolder", releaseDirectory.FullName}
                               });
        }
    }
}
