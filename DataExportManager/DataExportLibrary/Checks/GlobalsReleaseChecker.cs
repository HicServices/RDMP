using System;
using System.Linq;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Potential;
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
    public class GlobalsReleaseChecker
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IExtractionConfiguration[] _configurations;
        private readonly IMapsDirectlyToDatabaseTable _globalToCheck;

        public GlobalsReleaseChecker(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration[] extractionConfigurations, IMapsDirectlyToDatabaseTable globalToCheck)
        {
            _repositoryLocator = repositoryLocator;
            _configurations = extractionConfigurations;
            _globalToCheck = globalToCheck;
        }

        public ICheckable GetEvaluator()
        {
            var globalResult = _configurations.SelectMany(c => c.SupplementalExtractionResults)
                                                  .Distinct()
                                                  .FirstOrDefault(ser => ser.ExtractedId == _globalToCheck.ID && ser.GetExtractedType() == _globalToCheck.GetType());

            if (globalResult == null)
                return new NoGlobalReleasePotential(_repositoryLocator, null, _globalToCheck);
            
            //it's been extracted!, who extracted it?
            var destinationThatExtractedIt = (IExecuteDatasetExtractionDestination)new ObjectConstructor().Construct(globalResult.GetDestinationType());

            //destination tell us how releasable it is
            return destinationThatExtractedIt.GetGlobalReleasabilityEvaluator(_repositoryLocator, globalResult, _globalToCheck);
        }
    }
}