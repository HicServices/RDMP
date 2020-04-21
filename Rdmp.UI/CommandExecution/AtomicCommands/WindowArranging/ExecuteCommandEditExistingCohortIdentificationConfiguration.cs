// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditExistingCohortIdentificationConfiguration : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public CohortIdentificationConfiguration CohortIdConfig { get; set; }

        [UseWithObjectConstructor]
        public ExecuteCommandEditExistingCohortIdentificationConfiguration(IActivateItems activator,CohortIdentificationConfiguration cohortIdentificationConfiguration)
            : base(activator)
        {
            CohortIdConfig = cohortIdentificationConfiguration;
        }

        public ExecuteCommandEditExistingCohortIdentificationConfiguration(IActivateItems activator) : base(activator)
        {
            
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            CohortIdConfig = (CohortIdentificationConfiguration) target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "This will take you to the Cohort Identification Configurations list and allow you to Edit and Run the selected cohort.\r\n" +
                    "You must choose an item from the list before proceeding.";

        }

        public override void Execute()
        {
            base.Execute();

            var cic = CohortIdConfig ??
                      SelectOne<CohortIdentificationConfiguration>(BasicActivator.RepositoryLocator
                          .CatalogueRepository);
            
            if(cic == null)
                return;

            Activator.WindowArranger.SetupEditAnything(this, cic);
        }
    }
}