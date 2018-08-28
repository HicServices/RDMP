using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using LogManager = HIC.Logging.LogManager;

namespace DataLoadEngine.Checks.Checkers
{
    class MetadataLoggingConfigurationChecks : ICheckable
    {
        private readonly ILoadMetadata _loadMetadata;


        public MetadataLoggingConfigurationChecks(ILoadMetadata loadMetadata)
        {
            _loadMetadata = loadMetadata;
        }

        

        public void Check(ICheckNotifier notifier)
        {

            var catalogues = _loadMetadata.GetAllCatalogues().ToArray();

            //if there are no logging tasks defined on any Catalogues
            if (catalogues.Any() && catalogues.All(c => string.IsNullOrWhiteSpace(c.LoggingDataTask)))
            {
                string proposedName;
                
                bool fix;

                if(catalogues.Length == 1)
                {
                    proposedName = "Loading '" + catalogues[0] + "'";
                        fix = notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "Catalogue " + catalogues[0] + " does not have a logging task specified",
                                CheckResult.Fail, null, "Create a new Logging Task called '" + proposedName + "'?"));
                }
                else
                {
                    proposedName =  _loadMetadata.Name;

                    fix =
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "Catalogues " + string.Join(",",catalogues.Select(c=>c.Name)) + " do not have a logging task specified",
                                CheckResult.Fail, null, "Create a new Logging Task called '" + proposedName + "'?"));
                    
                }

                if (fix)
                    CreateNewLoggingTaskFor(notifier,catalogues, proposedName);
                else
                    return;
            }
            
            string distinctLoggingTask = null; 
            try
            {
                distinctLoggingTask = _loadMetadata.GetDistinctLoggingTask();
                notifier.OnCheckPerformed(new CheckEventArgs("All Catalogues agreed on a single Logging Task:" + distinctLoggingTask, CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Catalogues could not agreed on a single Logging Task", CheckResult.Fail, e));
            }

            try
            {
                var settings = _loadMetadata.GetDistinctLoggingDatabase();
                settings.TestConnection();
                notifier.OnCheckPerformed(new CheckEventArgs("Connected to logging architecture successfully", CheckResult.Success, null));


                if(distinctLoggingTask != null)
                {
                    LogManager lm = new LogManager(settings);
                    string[] dataTasks = lm.ListDataTasks();

                    if (dataTasks.Contains(distinctLoggingTask))
                        notifier.OnCheckPerformed(new CheckEventArgs("Found Logging Task " + distinctLoggingTask + " in Logging database",CheckResult.Success, null));
                    else
                    {
                        var fix = notifier.OnCheckPerformed(new CheckEventArgs("Could not find Logging Task " + distinctLoggingTask + " in Logging database", CheckResult.Fail, null, "Create Logging Task '" + distinctLoggingTask +"'"));
                        if(fix)
                            lm.CreateNewLoggingTaskIfNotExists(distinctLoggingTask);
                    }
                }

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could reach default logging server", CheckResult.Fail, e));
            }
        
        }

        private void CreateNewLoggingTaskFor(ICheckNotifier notifier,ICatalogue[] catalogues, string proposedName)
        {
            var catarepo = (CatalogueRepository) _loadMetadata.Repository;

            var serverIds = catalogues.Select(c => c.LiveLoggingServer_ID).Where(i=>i.HasValue).Distinct().ToArray();

            IExternalDatabaseServer loggingServer;

            if (serverIds.Any())
            {
                //a server is specified
                if(serverIds.Length != 1)
                    throw new Exception("Catalogues do not agree on a single logging server");

                //we checked for HasValue above see the WHERE in the linq
                loggingServer = catarepo.GetObjectByID<ExternalDatabaseServer>(serverIds[0].Value);
            }
            else
            {
                loggingServer = new ServerDefaults(catarepo).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
                
                if(loggingServer == null)
                    throw new Exception("There is no default logging server!");
            }

            var logManager = new LogManager(loggingServer);
            logManager.CreateNewLoggingTaskIfNotExists(proposedName);
            notifier.OnCheckPerformed(new CheckEventArgs("Created Logging Task '" + proposedName + "'",CheckResult.Success));

            foreach (Catalogue catalogue in catalogues.Cast<Catalogue>())
            {
                catalogue.LiveLoggingServer_ID = loggingServer.ID;
                catalogue.LoggingDataTask = proposedName;
                catalogue.SaveToDatabase();
            }


        }
    }
}
