// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Cohort;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCloneCohortIdentificationConfiguration : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private readonly IActivateItems activator;
        private CohortIdentificationConfiguration _cic;
        private Project _project;

        [ImportingConstructor]
        public ExecuteCommandCloneCohortIdentificationConfiguration(IActivateItems activator,CohortIdentificationConfiguration cic)
            : base(activator)
        {
            this.activator = activator;
            _cic = cic;
        }

        public override string GetCommandHelp()
        {
            return "Creates an exact copy of the Cohort Identification Configuration (query) including all cohort sets, patient index tables, parameters, filter containers, filters etc";
        }

        public ExecuteCommandCloneCohortIdentificationConfiguration(IActivateItems activator) : base(activator)
        {
            this.activator = activator;
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
                _cic = SelectOne<CohortIdentificationConfiguration>(activator.RepositoryLocator.CatalogueRepository);

            if(_cic == null)
                return;

            if (MessageBox.Show(
                    "This will create a 100% copy of the entire CohortIdentificationConfiguration including all datasets, " +
                    "filters, parameters and set operations. Are you sure this is what you want?",
                    "Confirm Cloning", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var checks = new PopupChecksUI("Cloning " + _cic, false);
                var clone = _cic.CreateClone(checks);

                if (_project != null) // clone the association
                    new ProjectCohortIdentificationConfigurationAssociation(
                                    Activator.RepositoryLocator.DataExportRepository,
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