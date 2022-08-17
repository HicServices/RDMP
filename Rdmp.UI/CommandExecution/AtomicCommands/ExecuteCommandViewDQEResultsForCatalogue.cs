// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.CatalogueSummary;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewDQEResultsForCatalogue : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        [UseWithObjectConstructor]
        public ExecuteCommandViewDQEResultsForCatalogue(IActivateItems activator,Catalogue catalogue)
            : base(activator)
        {
            SetTarget(catalogue);
        }

        public ExecuteCommandViewDQEResultsForCatalogue(IActivateItems activator):base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "View the results of all data quality engine runs that have ever been run on the dataset";
        }

        public override Image<Rgba32> GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.DQE;
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            
            //must have both of these things to be DQEd
            if (_catalogue.TimeCoverage_ExtractionInformation_ID == null)
            {
                SetImpossible("Catalogue does not have a Time Coverage column set");
                return this;
            }

            if (string.IsNullOrWhiteSpace(_catalogue.ValidatorXML))
            {
                SetImpossible("Catalogue does not have any validation rules configured");
                return this;
            }

            var dqeServer = Activator.ServerDefaults.GetDefaultFor(PermissableDefaults.DQE);

            if (dqeServer == null)
            {
                SetImpossible("There is no DQE server");
                return this;
            }

            if (!ServerHasAtLeastOneEvaluation(_catalogue))
                SetImpossible("DQE has never been run for Catalogue");

            return this;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<CatalogueDQEResultsUI, Catalogue>(_catalogue);
        }

        private bool ServerHasAtLeastOneEvaluation(Catalogue c)
        {
            return new DQERepository(Activator.RepositoryLocator.CatalogueRepository).HasEvaluations(c);
        }
    }
}