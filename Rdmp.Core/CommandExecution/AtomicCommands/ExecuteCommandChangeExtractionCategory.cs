// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandChangeExtractionCategory : BasicCommandExecution
    {
        ExtractionInformation[] _extractionInformations;
        private readonly ExtractionCategory? _category;

        [UseWithObjectConstructor]
        public ExecuteCommandChangeExtractionCategory(IBasicActivateItems activator,ExtractionInformation[] eis, ExtractionCategory? category = null) : base(activator)
        {
            eis = (eis??new ExtractionInformation[0]).Where(e => e != null).ToArray();

            if (eis.Length == 0)
                SetImpossible("No ExtractionInformations found");

            _extractionInformations = eis;
            this._category = category;
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

            var c = _category;

            if (c == null && BasicActivator.SelectValueType("New Extraction Category", typeof(ExtractionCategory), ExtractionCategory.Core, out object category))
                c = (ExtractionCategory)category;

            if (c == null)
                return;
                        
            foreach (var ei in _extractionInformations)
            {
                ei.ExtractionCategory = c.Value;
                ei.SaveToDatabase();
            }            

            //publish the root Catalogue
            Publish(_extractionInformations.First());
        }
    }
}
