using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.DataRelease
{
    public class FlatFileReleasePotential : ReleasePotential
    {
        public FlatFileReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ExtractionConfiguration configuration, ExtractableDataSet dataSet) : base(repositoryLocator, configuration, dataSet)
        {
        }

        protected override Releaseability GetSpecificAssessment()
        {
            ExtractDirectory = new FileInfo(ExtractionResults.DestinationDescription).Directory;
            if (FilesAreMissing())
                return Releaseability.ExtractFilesMissing;
            
            ThrowIfPollutionFoundInConfigurationRootExtractionFolder();
            return Releaseability.Undefined;// Assesment = SqlDifferencesVsLiveCatalogue() ? Releaseability.ColumnDifferencesVsCatalogue : Releaseability.Releaseable;
        }

        private bool FilesAreMissing()
        {
            ExtractFile = new FileInfo(ExtractionResults.DestinationDescription);
            var metadataFile = new FileInfo(ExtractionResults.DestinationDescription.Replace(".csv", ".docx"));

            if (!ExtractFile.Exists)
                return true;//extract is missing

            if (!ExtractFile.Extension.Equals(".csv"))
                throw new Exception("Extraction file had extension '" + ExtractFile.Extension + "' (expected .csv)");

            if (!metadataFile.Exists)
                return true;

            //see if there is any other polution in the extract directory
            FileInfo unexpectedFile = ExtractFile.Directory.EnumerateFiles().FirstOrDefault(f =>
                !(f.Name.Equals(ExtractFile.Name) || f.Name.Equals(metadataFile.Name)));

            if (unexpectedFile != null)
                throw new Exception("Unexpected file found in extract directory " + unexpectedFile.FullName + " (pollution of extract directory is not permitted)");

            DirectoryInfo unexpectedDirectory = ExtractFile.Directory.EnumerateDirectories().FirstOrDefault(d =>
                !(d.Name.Equals("Lookups") || d.Name.Equals("SupportingDocuments") || d.Name.Equals(SupportingSQLTable.ExtractionFolderName)));

            if (unexpectedDirectory != null)
                throw new Exception("Unexpected directory found in extraction directory " + unexpectedDirectory.FullName + " (pollution of extract directory is not permitted)");

            return false;
        }

        private void ThrowIfPollutionFoundInConfigurationRootExtractionFolder()
        {
            Debug.Assert(ExtractDirectory.Parent != null, "Dont call this method until you have determined that an extracted file was actually produced!");

            if (ExtractDirectory.Parent.GetFiles().Any())
                throw new Exception("The following pollutants were found in the extraction directory\" " +
                                    ExtractDirectory.Parent.FullName +
                                    "\" pollutants were:" +
                                    ExtractDirectory.Parent.GetFiles().Aggregate("", (s, n) => s + "\"" + n + "\""));
        }
    }
}