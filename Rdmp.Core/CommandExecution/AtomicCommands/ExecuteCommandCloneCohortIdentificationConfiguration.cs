// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCloneCohortIdentificationConfiguration : BasicCommandExecution, IAtomicCommandWithTarget
    {
        private CohortIdentificationConfiguration _cic;
        private Project _project;

        [UseWithObjectConstructor]
        public ExecuteCommandCloneCohortIdentificationConfiguration(IBasicActivateItems activator,CohortIdentificationConfiguration cic)
            : base(activator)
        {
            _cic = cic;
        }

        public override string GetCommandHelp()
        {
            return "Creates an exact copy of the Cohort Identification Configuration (query) including all cohort sets, patient index tables, parameters, filter containers, filters etc";
        }

        public ExecuteCommandCloneCohortIdentificationConfiguration(IBasicActivateItems activator) : base(activator)
        {
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is CohortIdentificationConfiguration)
                _cic = (CohortIdentificationConfiguration)target;

            if (target is Project)
                _project = (Project)target;

            return this;
        }

        public override void Execute()
        {
            base.Execute();

            if (_cic == null)
                _cic = SelectOne<CohortIdentificationConfiguration>(BasicActivator.RepositoryLocator.CatalogueRepository);

            if(_cic == null)
                return;

            if (YesNo("This will create a 100% copy of the entire CohortIdentificationConfiguration including all datasets, " +
                    "filters, parameters and set operations. Are you sure this is what you want?",
                    "Confirm Cloning"))
            {
                
                var clone = _cic.CreateClone(new ThrowImmediatelyCheckNotifier());

                if (_project != null) // clone the association
                    new ProjectCohortIdentificationConfigurationAssociation(
                                    BasicActivator.RepositoryLocator.DataExportRepository,
                                    _project,
                                    clone);

                //Load the clone up
                Publish(clone);
                if (_project != null)
                    Emphasise(_project);
                else
                    Emphasise(clone);
            }
        }
    }
}