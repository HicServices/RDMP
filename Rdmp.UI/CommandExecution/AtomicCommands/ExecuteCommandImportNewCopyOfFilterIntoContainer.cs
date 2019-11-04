// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ExtractionUIs.FilterUIs;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandImportNewCopyOfFilterIntoContainer : BasicUICommandExecution
    {
        private FilterCombineable _filterCombineable;
        private IContainer _targetContainer;

        public ExecuteCommandImportNewCopyOfFilterIntoContainer(IActivateItems activator,FilterCombineable filterCombineable, IContainer targetContainer) : base(activator)
        {
            _filterCombineable = filterCombineable;
            _targetContainer = targetContainer;

            //if source catalogue is known
            if(_filterCombineable.SourceCatalogueIfAny != null)
            {
                var targetCatalogue = targetContainer.GetCatalogueIfAny();
                
                if(targetCatalogue != null)
                    if(!_filterCombineable.SourceCatalogueIfAny.Equals(targetCatalogue))
                        SetImpossible("Cannot Import Filter from '" + _filterCombineable.SourceCatalogueIfAny + "' into '" + targetCatalogue +"'");

            }
        }

        public override void Execute()
        {
            base.Execute();

            var wizard = new FilterImportWizard();
            IFilter newFilter = wizard.Import(_targetContainer, _filterCombineable.Filter);
            if (newFilter != null)
            {
                _targetContainer.AddChild(newFilter);
                Publish((DatabaseEntity) _targetContainer);
            }
        }
    }
}