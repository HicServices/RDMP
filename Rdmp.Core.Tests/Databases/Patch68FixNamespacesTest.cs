using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.Databases;
using Tests.Common;

namespace Rdmp.Core.Tests.Databases
{
    class Patch68FixNamespacesTest:UnitTests
    {

        /// <summary>
        /// Tests the systems ability to migrate old class paths in deployed databases to the new namespaces as they exist in
        /// the refactored (modern) RDMP sln layout (before there were buckets of projects!)
        /// </summary>
        [Test]
        public void TestClassNameRefactoring()
        {
            CataloguePatcher p = new CataloguePatcher();

            var patch = p.GetAllPatchesInAssembly().Single(kvp=>kvp.Key == "068_FixNamespaces.sql").Value;

            Regex findSubsRegex = new Regex(@"REPLACE\(.*,'(.*)','(.*)'\)");
            
            Dictionary<string,string> substitutions = new Dictionary<string, string>();

            foreach (Match match in findSubsRegex.Matches(patch.EntireScript))
            {
                if(substitutions.ContainsKey(match.Groups[1].Value))
                    continue;

                substitutions.Add(match.Groups[1].Value,match.Groups[2].Value);
            }
            
            SetupMEF();

            foreach (var oldClass in ExpectedClasses)
            {
                Type foundBefore = MEF.GetTypeByNameFromAnyLoadedAssembly(oldClass);

                string newClass = oldClass;

                foreach (KeyValuePair<string, string> kvp in substitutions)
                    newClass = newClass.Replace(kvp.Key, kvp.Value);

                Type foundNow = MEF.GetTypeByNameFromAnyLoadedAssembly(newClass);

                Assert.IsNotNull(foundNow,"Patch did not work correctly for Type '" + oldClass +"' which after renaming became '" + newClass +"'");

            }
                

        }
        private string[] ExpectedClasses
            = new[]
            {
                "CachingEngine.PipelineExecution.Destinations.CacheFileGranularity",
                "CatalogueLibrary.Data.ColumnInfo",
                "CatalogueLibrary.Data.DataAccessCredentials",
                "CatalogueLibrary.Data.DataLoad.CacheArchiveType",
                "CatalogueLibrary.Data.DataLoad.LoadMetadata",
                "CatalogueLibrary.Data.EncryptedString",
                "CatalogueLibrary.Data.ExternalDatabaseServer",
                "CatalogueLibrary.Data.IExternalDatabaseServer",
                "CatalogueLibrary.Data.ILoadProgress",
                "CatalogueLibrary.Data.LoadProgress",
                "CatalogueLibrary.Data.Pipelines.Pipeline",
                "CatalogueLibrary.Data.Remoting.RemoteRDMP",
                "CatalogueLibrary.Data.StandardRegex",
                "CatalogueLibrary.Data.TableInfo",
                "CatalogueLibrary.Data.TableInfo",
                "CatalogueLibrary.ExitCodeType",
                "CatalogueLibrary.Repositories.CatalogueRepository",
                "DataExportLibrary.CohortCreationPipeline.Sources.CohortIdentificationConfigurationSource",
                "DataExportLibrary.CohortCreationPipeline.Sources.PatientIndexTableSource",
                "DataExportLibrary.DataRelease.ReleasePipeline.BasicDataReleaseDestination",
                "DataExportLibrary.DataRelease.ReleasePipeline.ReleaseFolderProvider",
                "DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations.ExecuteDatasetExtractionFlatFileDestination",
                "DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations.ExecuteExtractionToFlatFileType",
                "DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources.ExecuteCrossServerDatasetExtractionSource",
                "DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources.ExecuteDatasetExtractionSource",
                "DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources.ExecutePkSynthesizerDatasetExtractionSource",
                "DataLoadEngine.DataFlowPipeline.Components.CleanStrings",
                "DataLoadEngine.DataFlowPipeline.Destinations.DataTableUploadDestination",
                "FAnsi.DatabaseType",
                "LoadModules.Generic.Attachers.AnySeparatorFileAttacher",
                "LoadModules.Generic.Attachers.ExcelAttacher",
                "LoadModules.Generic.Attachers.FixedWidthAttacher",
                "LoadModules.Generic.Attachers.MDFAttacher",
                "LoadModules.Generic.Attachers.MdfAttachStrategy",
                "LoadModules.Generic.Attachers.RemoteTableAttacher",
                "LoadModules.Generic.DataFlowOperations.Aliases.AliasResolutionStrategy",
                "LoadModules.Generic.DataFlowOperations.ColumnBlacklister",
                "LoadModules.Generic.DataFlowOperations.ColumnRenamer",
                "LoadModules.Generic.DataFlowOperations.ExtractCatalogueMetadata",
                "LoadModules.Generic.DataFlowOperations.ReleaseMetadata",
                "LoadModules.Generic.DataFlowOperations.Swapping.ColumnSwapper",
                "LoadModules.Generic.DataFlowOperations.Transposer",
                "LoadModules.Generic.DataFlowSources.BadDataHandlingStrategy",
                "LoadModules.Generic.DataFlowSources.DelimitedFlatFileDataFlowSource",
                "LoadModules.Generic.DataFlowSources.ExcelDataFlowSource",
                "LoadModules.Generic.DataFlowSources.ExplicitTypingCollection",
                "LoadModules.Generic.DataProvider.ImportFilesDataProvider",
                "LoadModules.Generic.DataProvider.WebServiceConfiguration",
                "LoadModules.Generic.FTP.SFTPDownloader",
                "LoadModules.Generic.FileOperations.FileUnzipper",
                "LoadModules.Generic.LoadProgressUpdating.DataLoadProgressUpdateInfo",
                "LoadModules.Generic.Mutilators.Coalescer",
                "LoadModules.Generic.Mutilators.Distincter",
                "LoadModules.Generic.Mutilators.PrematureLoadEndCondition",
                "LoadModules.Generic.Mutilators.PrematureLoadEnder",
                "LoadModules.Generic.Mutilators.PrimaryKeyCollisionResolverMutilation",
                "LoadModules.Generic.Mutilators.SafePrimaryKeyCollisionResolverMutilation",
                "LoadModules.Generic.Mutilators.TableVarcharMaxer",
                "LoadModules.Generic.Web.WebFileDownloader"
            };

    }
}
