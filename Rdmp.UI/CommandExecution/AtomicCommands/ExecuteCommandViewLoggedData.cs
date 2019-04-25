// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.Logging;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.LogViewer.Tabs;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Rdmp.UI.CommandExecution.AtomicCommands
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
            _loggingServers = Activator.RepositoryLocator.CatalogueRepository.GetAllDatabases<LoggingDatabasePatcher>();

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

            LoggingTabUI loggingTabUI = null;

            if(server != null)
                switch (_target)
                {
                    case LoggingTables.DataLoadTask:
                        loggingTabUI = Activator.Activate<LoggingTasksTabUI, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.DataLoadRun:
                        loggingTabUI = Activator.Activate<LoggingRunsTabUI, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.ProgressLog:
                        loggingTabUI = Activator.Activate<LoggingProgressMessagesTabUI, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.FatalError:
                        loggingTabUI = Activator.Activate<LoggingFatalErrorsTabUI, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.TableLoadRun:
                        loggingTabUI = Activator.Activate<LoggingTableLoadsTabUI, ExternalDatabaseServer>(server);
                        break;
                    case LoggingTables.DataSource:
                        loggingTabUI = Activator.Activate<LoggingDataSourcesTabUI, ExternalDatabaseServer>(server);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            loggingTabUI.SetFilter(_filter);
        }

        public override string GetCommandName()
        {
            return UsefulStuff.PascalCaseStringToHumanReadable(_target.ToString());
        }
    }
}