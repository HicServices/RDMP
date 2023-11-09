// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Linq;
using static Rdmp.Core.ReusableLibraryCode.Diff;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    internal class Args : RDMPCommandLineOptions { }
    internal class ExecuteCommandExportPlatformDatabasesToYaml : BasicCommandExecution, IAtomicCommand
    {
        private readonly TableRepository _catalogueRepository;
        private readonly TableRepository _dataExportRepository;
        private readonly string _outputDirectory;
        private readonly IBasicActivateItems _activator;

        public ExecuteCommandExportPlatformDatabasesToYaml(IBasicActivateItems activator, [DemandsInitialization("Where the yaml file should be created")] string outputDirectory)
        {

            _catalogueRepository = activator.RepositoryLocator.CatalogueRepository as TableRepository;
            _dataExportRepository = activator.RepositoryLocator.DataExportRepository as TableRepository;
            _outputDirectory = outputDirectory;
            _activator = activator;

        }

        public override void Execute()
        {
            base.Execute();
            Args args = new Args()
            {
                Dir = _outputDirectory,
            };
            var yamlRespositoryLocator = args.GetRepositoryLocator();
            foreach (var t in _catalogueRepository.GetCompatibleTypes())
            {
                foreach(var o in _activator.GetRepositoryFor(t).GetAllObjects(t))
                {
                    yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(o);

                }
            }
            foreach (var t in _dataExportRepository.GetCompatibleTypes())
            {
                foreach (var o in _activator.GetRepositoryFor(t).GetAllObjects(t))
                {
                    yamlRespositoryLocator.DataExportRepository.SaveToDatabase(o);

                }
            }

        }
    }
}
