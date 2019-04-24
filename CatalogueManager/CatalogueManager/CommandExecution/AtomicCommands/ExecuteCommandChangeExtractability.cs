// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.Core.DataExport.Providers;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandChangeExtractability:BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;
        private bool _isExtractable;

        public ExecuteCommandChangeExtractability(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
            var status = catalogue.GetExtractabilityStatus(activator.RepositoryLocator.DataExportRepository);
            if (status == null)
            {
                SetImpossible("We don't know whether Catalogue is extractable or not (possibly no Data Export database is available)");
                return;
            }

            if(status.IsProjectSpecific)
            {
                SetImpossible("Cannot change the extractability because it is configured as a 'Project Specific Catalogue'");
                return;
            }

            _isExtractable = status.IsExtractable;
        }

        public override string GetCommandName()
        {
            return _isExtractable?"Mark Not Extractable":"Mark Extractable";
        }

        public override string GetCommandHelp()
        {

            if (_isExtractable)
                return "Prevent dataset from being released in Project extracts.  This fails if it is already part of any ExtractionConfigurations";
            
            return @"Enable dataset linkage\extraction in Project extracts.  This requires that at least one column be marked IsExtractionIdentifier";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableDataSet, _isExtractable?OverlayKind.Delete:OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            if (_isExtractable)
            {
                var extractabilityRecord = ((DataExportChildProvider) Activator.CoreChildProvider).ExtractableDataSets.Single(ds => ds.Catalogue_ID == _catalogue.ID);
                extractabilityRecord.DeleteInDatabase();
                Publish(_catalogue);
            }
            else
            {
                 new ExtractableDataSet(Activator.RepositoryLocator.DataExportRepository, _catalogue);
                Publish(_catalogue);

            }
        }
    }
}
