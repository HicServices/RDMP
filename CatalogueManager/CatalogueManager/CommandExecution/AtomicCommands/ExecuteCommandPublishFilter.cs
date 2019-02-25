// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandPublishFilter : BasicUICommandExecution, IAtomicCommand
    {
        private readonly IFilter _filter;
        private Catalogue _catalogue;
        private ExtractionInformation[] _allExtractionInformations;

        public ExecuteCommandPublishFilter(IActivateItems activator, IFilter filter,Catalogue targetCatalogue):base(activator)
        {
            _filter = filter;
            _catalogue = targetCatalogue;

            if (filter is ExtractionFilter)
            {
                SetImpossible("Filter is already a master Catalogue filter");
                return;
            }


            if (_catalogue == null)
            {
                SetImpossible("No Catalogue is associated with filter");
                return;
            }

            _allExtractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            if (!_allExtractionInformations.Any())
                SetImpossible("Cannot publish filter because Catalogue " + _catalogue + " does not have any ExtractionInformations (extractable columns) we could associate it with");

            string reason;
            if (!FilterImporter.IsProperlyDocumented(filter, out reason))
                SetImpossible("Filter is not properly documented:" + reason);
        }

        public override void Execute()
        {
            base.Execute();

            if (_catalogue == null)
                _catalogue = SelectOne<Catalogue>(Activator.RepositoryLocator.CatalogueRepository);

            var toAddTo = SelectOne(_allExtractionInformations);

            if(toAddTo != null)
            {
                //see if there is one with the same name that for some reason we are not known to be a child of already
                var duplicate = toAddTo.ExtractionFilters.SingleOrDefault(f => f.Name.Equals(_filter.Name));

                if (duplicate != null)
                {
                    var drMarkSame = MessageBox.Show("There is already a filter called " + _filter.Name +
                                                     " in ExtractionInformation " + toAddTo + " do you want to mark this filter as a child of that master filter?",
                        "Duplicate, mark these as the same?", MessageBoxButtons.YesNo);

                    if (drMarkSame == DialogResult.Yes)
                    {
                        _filter.ClonedFromExtractionFilter_ID = duplicate.ID;
                        _filter.SaveToDatabase();
                    }
                    return;
                }
                
                new FilterImporter(new ExtractionFilterFactory(toAddTo), null).ImportFilter(_filter, null);
                MessageBox.Show("Publish successful");
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}