using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;

using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Migration;
using DataQualityEngine.Data;
using DataQualityEngine.Reports;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.DQE
{
    /// <summary>
    /// Identifies Catalogues which have validation rules on them but have yet to have the Data Quality Engine run on them (or it has been a long time since
    /// the last DQE run happened).  
    /// 
    /// <para> Used by DQEAutomationSource to decide when a new AutomatedDQERun can be started. </para>
    /// </summary>
    public class DQERunFinder
    {
        private readonly CatalogueRepository _catalogueRepository;
        private readonly int _dqeDaysBetweenEvaluations;
        private readonly IDataLoadEventListener _listener;

        public DQERunFinder(CatalogueRepository catalogueRepository, int dqeDaysBetweenEvaluations, IDataLoadEventListener listener)
        {
            _catalogueRepository = catalogueRepository;
            _dqeDaysBetweenEvaluations = dqeDaysBetweenEvaluations;
            _listener = listener;
        }


        enum AutomationDQEJobSelectionStrategy
        {
            MostRecentlyLoadedDataset,
            DatasetWithMostOutOfDateDQEResults
        }

        public Catalogue SuggestRun()
        {
            var availableCatalogues = _catalogueRepository.GetAllCataloguesWithAtLeastOneExtractableItem().Where(c =>
                !c.IsColdStorageDataset
                &&
                !c.IsDeprecated).ToArray();

            if (availableCatalogues.Length == 0)
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No catalogue available for DQE (must be not in cold storage, not deprecated and not locked)"));
                return null;
            }

            var dqeRepository = new DQERepository(_catalogueRepository);

            List<Tuple<Catalogue, DateTime>> datasetsByDate;
            Tuple<Catalogue, DateTime> pairToReturn;

            switch (AutomationDQEJobSelectionStrategy.MostRecentlyLoadedDataset)
            {
                case AutomationDQEJobSelectionStrategy.MostRecentlyLoadedDataset:

                    datasetsByDate = new List<Tuple<Catalogue, DateTime>>();
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, "Finding with strategy: MostRecentlyLoadedDataset."));

                    //First find whether theres a valid candidate based on dataset load date
                    foreach (var c in availableCatalogues)
                    {
                        var loggingServer = c.LiveLoggingServer;
                        if (loggingServer == null || string.IsNullOrWhiteSpace(c.LoggingDataTask)) //Catalogue is not logged
                        {
                            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace,
                                String.Format("Catalogue {0} has no logging... skipping.", c)));
                            continue;
                        }

                        var server = DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging);

                        //Get the last successful load attempt
                        LogManager lm = new LogManager(server);
                        var dateOfLastLoad = lm.GetDateOfLastLoadAttemptForTask(c.LoggingDataTask,true);

                        //its never been loaded
                        if(dateOfLastLoad == null)
                        {
                            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace,
                                String.Format("Catalogue {0} has never been loaded... skipping.", c)));
                            continue;
                        }

                        //it has been recently loaded BUT has the DQE also been run recently?
                        var candidate = dqeRepository.GetMostRecentEvaluationFor(c);
                            
                        //the DQE has been run on this dataset at least once!
                        if(candidate != null)
                        {
                            //has the DQE been run since the data load ended
                            if(candidate.DateOfEvaluation > dateOfLastLoad)
                            {
                                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace, 
                                    String.Format("Catalogue {0} has no new data loaded after last evaluation... skipping.", candidate.Catalogue)));
                                continue;
                            }

                            //well the DQE has not been run since the data load completed but it was run in the lag window so we probably shouldn't run the DQE again
                            if(IsInQuietPeriod(candidate.DateOfEvaluation))
                            {
                                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace,
                                    String.Format("Catalogue {0} has been evaluated less than {1} day(s) ago... skipping.", candidate.Catalogue, _dqeDaysBetweenEvaluations)));
                                continue;
                            }
                        }

                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace,
                            String.Format("Catalogue {0} is ready for evaluation... adding.", c)));
                        datasetsByDate.Add(new Tuple<Catalogue, DateTime>(c, dateOfLastLoad.Value));
                    }

                    var ordered = datasetsByDate.OrderBy(p => p.Item2);
                    pairToReturn = ordered.FirstOrDefault(p => CanBeDQEd(p.Item1));

                    //if we found a recently loaded catalogue
                    if (pairToReturn != null)
                    {
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                            String.Format("Found most recently loaded catalogue: {0}", pairToReturn.Item1)));
                        return pairToReturn.Item1;
                    }
                    else
                    {
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                            "No datasets are loaded or they have all been recently evaluated by DQE or something else. Let's fallback to the DatasetWithMostOutOfDateDQEResults Strategy."));
                        goto case AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults;//No dasets are loaded or they have all been recently evaluated by DQE or something else.  Lets fallback on the other Strategy instead
                    }
                    
                case AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults:

                    datasetsByDate = new List<Tuple<Catalogue, DateTime>>();
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, "Finding with strategy: DatasetWithMostOutOfDateDQEResults."));

                    foreach (var c in availableCatalogues)
                    {
                        var candidate = dqeRepository.GetMostRecentEvaluationFor(c);

                        //if it has never ben evaluated 
                        if (candidate == null)
                            datasetsByDate.Add(new Tuple<Catalogue, DateTime>(c, DateTime.MinValue));
                        else
                        {
                            //it was recently evaluated
                            if (IsInQuietPeriod(candidate.DateOfEvaluation))
                            {
                                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace,
                                    String.Format("Catalogue {0} has been evaluated less than {1} day(s) ago... skipping.", candidate.Catalogue, _dqeDaysBetweenEvaluations)));
                                continue;//so do not add it
                            }
                            
                            //it was evaluated ages ago
                            datasetsByDate.Add(new Tuple<Catalogue, DateTime>(c, candidate.DateOfEvaluation));
                        }
                    }

                    pairToReturn = datasetsByDate.OrderBy(p => p.Item2).FirstOrDefault(p => CanBeDQEd(p.Item1));
                    if (pairToReturn == null)
                    {
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                            "All datasets have been recently evaluated by DQE. Ending."));
                        return null;
                    }
                    
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        String.Format("Found oldest evaluated catalogue: {0}", pairToReturn.Item1)));
                    return pairToReturn.Item1;
            }
        }

        private bool CanBeDQEd(Catalogue catalogue)
        {
            //see if it can be checked
            var report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID);

            var checker = new ToMemoryCheckNotifier();
            report.Check(checker);

            if (_listener != null)
            {
                foreach (var check in checker.Messages)
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, check.Message, check.Ex));
                }
            }

            return checker.GetWorst() != CheckResult.Fail;
        }

        private bool IsInQuietPeriod(DateTime latestEvaluationDate)
        {
            //if number of days since last evaluation is less than the DQE every X days setting then we are in quiet period and shouldn't run catalogue
            return  (int) DateTime.Now.Subtract(latestEvaluationDate).TotalDays < _dqeDaysBetweenEvaluations;
        }
    }
}
