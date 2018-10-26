using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    /// <summary>
    /// Checks the release state of the Globals that should have been extracted as part of the given <see cref="ExtractionConfiguration"/>.  If they
    /// are missing then the overall release should not be run.
    /// </summary>
    public class GlobalsReleaseChecker : ICheckable
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IExtractionConfiguration[] _configurations;
        private readonly IMapsDirectlyToDatabaseTable _globalToCheck;

        public GlobalsReleaseChecker(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration[] extractionConfigurations, IMapsDirectlyToDatabaseTable globalToCheck = null)
        {
            _repositoryLocator = repositoryLocator;
            _configurations = extractionConfigurations;
            _globalToCheck = globalToCheck;
        }

        public ICheckable GetEvaluator()
        {
            var globalResult = _configurations.SelectMany(c => c.SupplementalExtractionResults)
                                                  .Distinct()
                                                  .FirstOrDefault(ser => ser.ReferencedObjectID == _globalToCheck.ID && ser.GetExtractedType() == _globalToCheck.GetType());

            if (globalResult == null)
                return new NoGlobalReleasePotential(_repositoryLocator, null, _globalToCheck);
            
            //it's been extracted!, who extracted it?
            var destinationThatExtractedIt = (IExecuteDatasetExtractionDestination)new ObjectConstructor().Construct(globalResult.GetDestinationType());

            //destination tell us how releasable it is
            return destinationThatExtractedIt.GetGlobalReleasabilityEvaluator(_repositoryLocator, globalResult, _globalToCheck);
        }

        public void Check(ICheckNotifier notifier)
        {
            // checks for pollution in the globals directory
            foreach (var extractionConfiguration in _configurations)
            {
                var allExtracted = extractionConfiguration.SupplementalExtractionResults.Where(ser => IsValidPath(ser.DestinationDescription));
                var extractDir = extractionConfiguration.GetProject().ExtractionDirectory;
                var folder = new ExtractionDirectory(extractDir, extractionConfiguration).GetGlobalsDirectory();

                var unexpectedDirectories = folder.EnumerateDirectories().Where(d => !d.Name.Equals("SupportingDocuments")).ToList();

                if (unexpectedDirectories.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("Unexpected directories found in extraction directory (" + 
                                                                  String.Join(",", unexpectedDirectories.Select(d => d.FullName)) + 
                                                                  ". Pollution of extract directory is not permitted.", CheckResult.Fail));

                var unexpectedFiles = folder.EnumerateFiles("*.*", SearchOption.AllDirectories).Where(f => allExtracted.All(ae => ae.DestinationDescription != f.FullName)).ToList();

                if (unexpectedFiles.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("Unexpected files found in extract directory (" +
                                                                 String.Join(",", unexpectedFiles.Select(d => d.FullName)) + 
                                                                 "). Pollution of extract directory is not permitted.", CheckResult.Fail));
            }
        }

        private bool IsValidPath(string path)
        {
            try
            {
                var fi = new FileInfo(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}