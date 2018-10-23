using System;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.LogViewer;
using CatalogueManager.LogViewer.Tabs;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewLoggedData : BasicUICommandExecution,IAtomicCommand
    {
        private readonly LoggingTables _target;
        protected readonly LogViewerFilter _filter;
        protected ExternalDatabaseServer[] _loggingServers;

        public ExecuteCommandViewLoggedData(IActivateItems activator,LoggingTables target = LoggingTables.DataLoadTask, LogViewerFilter filter = null) : base(activator)
        {
            _target = target;
            _filter = filter ?? new LogViewerFilter();
            _loggingServers = Activator.RepositoryLocator.CatalogueRepository.GetAllTier2Databases(Tier2DatabaseType.Logging);

            if(!_loggingServers.Any())
                SetImpossible("There are no logging servers");
        }

        public override string GetCommandHelp()
        {
            return "View the hierarchical audit log of all data flows through RDMP (data load, extraction, dqe runs etc) including progress, errors etc";
        }

        public override void Execute()
        {
            base.Execute();

            var server = SelectOne(_loggingServers);

            LoggingTab loggingTab = null;

            if(server != null)
                switch (_target)
                {
                    case LoggingTables.DataLoadTask:
                        loggingTab = Activator.Activate<LoggingTasksTab, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.DataLoadRun:
                        loggingTab = Activator.Activate<LoggingRunsTab, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.ProgressLog:
                        loggingTab = Activator.Activate<LoggingProgressMessagesTab, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.FatalError:
                        loggingTab = Activator.Activate<LoggingFatalErrorsTab, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.TableLoadRun:
                        loggingTab = Activator.Activate<LoggingTableLoadsTab, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.DataSource:
                        loggingTab = Activator.Activate<LoggingDataSourcesTab, ExternalDatabaseServer>(server);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            loggingTab.SetFilter(_filter);
        }

        public override string GetCommandName()
        {
            return UsefulStuff.PascalCaseStringToHumanReadable(_target.ToString());
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}