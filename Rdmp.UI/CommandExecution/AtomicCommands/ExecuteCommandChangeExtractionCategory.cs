// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;


namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    class ExecuteCommandChangeExtractionCategory : BasicUICommandExecution
    {
        ExtractionInformation[] _extractionInformations;
        
        public ExecuteCommandChangeExtractionCategory(IActivateItems activator,params ExtractionInformation[] eis) : base(activator)
        {
            eis = (eis??new ExtractionInformation[0]).Where(e => e != null).ToArray();

            if (eis.Length == 0)
                SetImpossible("No ExtractionInformations found");

            _extractionInformations = eis;
        }

        public override string GetCommandName()
        {
            if(_extractionInformations == null || _extractionInformations.Length <= 1)
                return "Set ExtractionCategory";

            return "Set ALL to ExtractionCategory";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionInformation);
        }

        public override void Execute()
        {
            base.Execute();

            var dlg = new SimpleDialogs.PickOneOrCancelDialog<ExtractionCategory>(
                Enum.GetValues(typeof(ExtractionCategory))
                    .OfType<ExtractionCategory>()
                    .Where(v=>v!= ExtractionCategory.Any).ToArray(),
                "New Extraction Category",
                (o)=>Activator.CoreIconProvider.GetImage(o),
                null
                );


            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var ei in _extractionInformations)
                {
                    ei.ExtractionCategory = dlg.Picked;
                    ei.SaveToDatabase();
                }
            }

            //publish the root Catalogue
            Publish(_extractionInformations.First().CatalogueItem.Catalogue);
        }
    }
}
