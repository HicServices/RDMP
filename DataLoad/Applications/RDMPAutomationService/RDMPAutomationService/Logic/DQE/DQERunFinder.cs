using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Migration;
using DataQualityEngine.Data;
using DataQualityEngine.Reports;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace RDMPAutomationService.Logic.DQE
{
    public class DQERunFinder
    {
        private readonly CatalogueRepository _catalogueRepository;
        private AutomationDQEJobSelectionStrategy _strategy;
        private readonly int _dqeDaysBetweenEvaluations;

        public DQERunFinder(CatalogueRepository catalogueRepository, AutomationDQEJobSelectionStrategy strategy, int dqeDaysBetweenEvaluations)
        {
            _catalogueRepository = catalogueRepository;
            _strategy = strategy;
            _dqeDaysBetweenEvaluations = dqeDaysBetweenEvaluations;
        }

        public Catalogue SuggestRun()
        {
            var lockedCatalogues = _catalogueRepository.GetAllAutomationLockedCatalogues();
            var availableCatalogues = _catalogueRepository.GetAllCataloguesWithAtLeastOneExtractableItem().Where(c=>
                !c.IsColdStorageDataset
                &&
                !c.IsDeprecated
                &&
                !lockedCatalogues.Contains(c)).ToArray();

            var dqeRepository = new DQERepository(_catalogueRepository);


            List<Pair<Catalogue, DateTime>> datasetsByDate;
            Pair<Catalogue, DateTime> pairToReturn;

            switch (_strategy)
            {
                case AutomationDQEJobSelectionStrategy.MostRecentlyLoadedDataset:
                    
                    datasetsByDate = new List<Pair<Catalogue,DateTime>>();

                    //First find whether theres a valid candidate based on dataset load date
                    foreach (var c in availableCatalogues)
                    {
                        var loggingServer = c.LiveLoggingServer;
                        if (loggingServer == null || string.IsNullOrWhiteSpace(c.LoggingDataTask)) //Catalogue is not logged
                            continue;//try the next catalogue

                        var server = DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging);

                        //Get the last successful load attempt
                        LogManager lm = new LogManager(server);
                        var dateOfLastLoad = lm.GetDateOfLastLoadAttemptForTask(c.LoggingDataTask,true);

                        //its never been loaded
                        if(dateOfLastLoad == null)
                            continue;//try the next catalogue

                        //it has been recently loaded BUT has the DQE also been run recently?
                        var candidate = dqeRepository.GetMostRecentEvaluationFor(c);
                            
                        //the DQE has been run on this dataset at least once!
                        if(candidate != null)
                        {
                            //has the DQE been run since the data load ended
                            if(candidate.DateOfEvaluation > dateOfLastLoad)
                                continue;

                            //well the DQE has not been run since the data load completed but it was run in the lag window so we probably shouldn't run the DQE again
                            if(IsInQuietPeriod(candidate.DateOfEvaluation))
                                continue;
                        }

                        datasetsByDate.Add(new Pair<Catalogue, DateTime>(c, dateOfLastLoad.Value));
                    }

                    var ordered = datasetsByDate.OrderBy(p => p.Second);
                    pairToReturn = ordered.FirstOrDefault(p => CanBeDQEd(p.First));

                    //if we found a recently loaded catalogue
                    if (pairToReturn != null)
                        return pairToReturn.First;
                    else
                        goto case AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults;//No dasets are loaded or they have all been recently evaluated by DQE or something else.  Lets fallback on the other Strategy instead
                    
                case AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults:

                    datasetsByDate = new List<Pair<Catalogue, DateTime>>();

                    foreach (var c in availableCatalogues)
                    {
                        var candidate = dqeRepository.GetMostRecentEvaluationFor(c);

                        //if it has never ben evaluated 
                        if (candidate == null)
                            datasetsByDate.Add(new Pair<Catalogue, DateTime>(c, DateTime.MinValue));
                        else
                        {
                            //it was recently evaluated
                            if (IsInQuietPeriod(candidate.DateOfEvaluation))
                                continue;//so do not add it
                            
                            //it was evaluated ages ago
                            datasetsByDate.Add(new Pair<Catalogue, DateTime>(c, candidate.DateOfEvaluation));
                        }
                    }

                    pairToReturn = datasetsByDate.OrderBy(p => p.Second).FirstOrDefault(p => CanBeDQEd(p.First));
                    return pairToReturn == null? null : pairToReturn.First;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool CanBeDQEd(Catalogue catalogue)
        {
            //see if it can be checked
            CatalogueConstraintReport report = new CatalogueConstraintReport(catalogue, MigrationColumnSet.DataLoadRunField);

            var checker = new ToMemoryCheckNotifier();
            report.Check(checker);

            return checker.GetWorst() != CheckResult.Fail;
        }

        private bool IsInQuietPeriod(DateTime latestEvaluationDate)
        {
            //if number of days since last evaluation is less than the DQE every X days setting then we are in quiet period and shouldn't run catalogue
            return  (int) DateTime.Now.Subtract(latestEvaluationDate).TotalDays < _dqeDaysBetweenEvaluations;
        }
    }
}
