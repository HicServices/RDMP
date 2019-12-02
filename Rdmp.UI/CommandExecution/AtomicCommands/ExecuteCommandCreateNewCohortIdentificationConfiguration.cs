// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Wizard;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Creates a new persistent database query configuration for identifying cohort sets of patients.
    /// </summary>
    public class ExecuteCommandCreateNewCohortIdentificationConfiguration: BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Project _associateWithProject;

        public ExecuteCommandCreateNewCohortIdentificationConfiguration(IActivateItems activator) : base(activator)
        {
            if(!activator.CoreChildProvider.AllCatalogues.Any())
                SetImpossible("There are no datasets loaded yet into RDMP");

            UseTripleDotSuffix = true;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration,OverlayKind.Add);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _associateWithProject = target as Project;
            return this;
        }

        public override void Execute()
        {
            base.Execute();
            
            CohortIdentificationConfiguration cic;

            if (UserSettings.ShowCohortWizard)
            {
                var wizard = new CreateNewCohortIdentificationConfigurationUI(Activator);

                if (wizard.ShowDialog() == DialogResult.OK)
                    cic = wizard.CohortIdentificationCriteriaCreatedIfAny;
                else 
                    return;
            }
            else
            {
                if (TypeText("Cohort Query Name", "Cohort Name", out string name))
                {
                    cic = new CohortIdentificationConfiguration(Activator.RepositoryLocator.CatalogueRepository, name);
                    cic.CreateRootContainerIfNotExists();
                    var exclusion = cic.RootCohortAggregateContainer;
                    exclusion.Name = "Exclusion Criteria";
                    exclusion.Operation = SetOperation.EXCEPT;
                    exclusion.SaveToDatabase();

                    var inclusion = new CohortAggregateContainer(Activator.RepositoryLocator.CatalogueRepository, SetOperation.UNION);
                    inclusion.Name = "Inclusion Criteria";
                    inclusion.SaveToDatabase();

                    exclusion.AddChild(inclusion);
                }
                else
                    return;
            }

            if (cic == null)
                return;

            if (_associateWithProject != null)
            {
                var assoc = _associateWithProject.AssociateWithCohortIdentification(cic);
                Publish(assoc);
                Emphasise(assoc, int.MaxValue);

            }
            else
            {
                Publish(cic);
                Emphasise(cic, int.MaxValue);
            }

            Activate(cic);
        }


        public override string GetCommandHelp()
        {
            return
                "This will open a window which will guide you in the steps for creating a Cohort based on Inclusion and Exclusion criteria.\r\n" +
                "You will be asked to choose one or more Dataset and the associated column filters to use as inclusion or exclusion criteria for the cohort.";
        }
    }
}