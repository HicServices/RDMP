using System;
using System.Collections.Generic;
using System.IO;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.DataRelease.Audit
{
    /// <summary>
    /// Records the fact that a given extracted dataset has been released.  It audits the user performing the release, the environmental release potential,
    /// destination directory etc.
    /// 
    /// <para>This is done by linking the CumulativeExtractionResult with a record in the ReleaseLog.  Each SelectedDataset in an ExtractionConfiguration
    /// can only have 1 CumulativeExtractionResult at a time (it is a record of the last extracted SQL etc - See CumulativeExtractionResult) and there can be 
    /// only 1 ReleaseLog entry per CumulativeExtractionResult.  This means that once a dataset has been released it cannot be extracted/released again (this
    /// is intended behaviour).  If you want to re run a released ExtractionConfiguration then you should clone it.</para>
    /// </summary>
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
