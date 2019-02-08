// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandImportNewCopyOfFilterIntoContainer : BasicUICommandExecution
    {
        private FilterCommand _filterCommand;
        private IContainer _targetContainer;

        public ExecuteCommandImportNewCopyOfFilterIntoContainer(IActivateItems activator,FilterCommand filterCommand, IContainer targetContainer) : base(activator)
        {
            _filterCommand = filterCommand;
            _targetContainer = targetContainer;

            //if source catalogue is known
            if(_filterCommand.SourceCatalogueIfAny != null)
            {
                var targetCatalogue = targetContainer.GetCatalogueIfAny();
                
                if(targetCatalogue != null)
                    if(!_filterCommand.SourceCatalogueIfAny.Equals(targetCatalogue))
                        SetImpossible("Cannot Import Filter from '" + _filterCommand.SourceCatalogueIfAny + "' into '" + targetCatalogue +"'");

            }
        }

        public override void Execute()
        {
            base.Execute();

            var wizard = new FilterImportWizard();
            IFilter newFilter = wizard.Import(_targetContainer, _filterCommand.Filter);
            if (newFilter != null)
            {
                _targetContainer.AddChild(newFilter);
                Publish((DatabaseEntity) _targetContainer);
            }
        }
    }
}