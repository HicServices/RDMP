using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LogViewer;
using CatalogueManager.LogViewer.Tabs;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandViewLoggedData : BasicUICommandExecution,IAtomicCommand
    {
        private readonly LogViewerNavigationTarget _target;
        private readonly LogViewerFilter _filter;
        private ExternalDatabaseServer[] _loggingServers;

        public ExecuteCommandViewLoggedData(IActivateItems activator,LogViewerNavigationTarget target = LogViewerNavigationTarget.DataLoadTasks, LogViewerFilter filter = null) : base(activator)
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
                    case LogViewerNavigationTarget.DataLoadTasks:
                        loggingTab = Activator.Activate<LoggingTasksTab, ExternalDatabaseServer>(server);
                        break;
                    case LogViewerNavigationTarget.DataLoadRuns:
                        loggingTab = Activator.Activate<LoggingRunsTab, ExternalDatabaseServer>(server);
                        break;
                    case LogViewerNavigationTarget.ProgressMessages:
                        loggingTab = Activator.Activate<LoggingProgressMessagesTab, ExternalDatabaseServer>(server);
                        break;
                    case LogViewerNavigationTarget.FatalErrors:
                        loggingTab = Activator.Activate<LoggingFatalErrorsTab, ExternalDatabaseServer>(server);
                        break;
                    case LogViewerNavigationTarget.TableLoadRuns:
                        loggingTab = Activator.Activate<LoggingTableLoadsTab, ExternalDatabaseServer>(server);
                        break;
                    case LogViewerNavigationTarget.DataSources:
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