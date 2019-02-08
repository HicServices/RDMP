// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueManager.ItemActivation;
using CommandLine;
using RDMPAutomationService.Options.Abstracts;

namespace CatalogueManager.CommandExecution.AtomicCommands.Automation
{
    public abstract class AutomationCommandExecution : BasicUICommandExecution
    {
        protected readonly Func<RDMPCommandLineOptions> CommandGetter;
        public const string AutomationServiceExecutable = "RDMPAutomationService.exe";

        protected AutomationCommandExecution(IActivateItems activator, Func<RDMPCommandLineOptions> commandGetter) : base(activator)
        {
            CommandGetter = commandGetter;
        }

        protected string GetCommandText()
        {
            Parser p = new Parser();
            var options = CommandGetter();

            PopulateConnectionStringOptions(options);

            return AutomationServiceExecutable + " " + p.FormatCommandLine(options);
        }

        private void PopulateConnectionStringOptions(RDMPCommandLineOptions options)
        {
            if (Activator == null)
                return;

            if (string.IsNullOrWhiteSpace(options.CatalogueConnectionString))
                options.CatalogueConnectionString = Activator.RepositoryLocator.CatalogueRepository.ConnectionStringBuilder.ConnectionString;

            if (string.IsNullOrWhiteSpace(options.DataExportConnectionString))
                options.DataExportConnectionString = Activator.RepositoryLocator.DataExportRepository.ConnectionStringBuilder.ConnectionString;
        }
    }
}