// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandSetExtractionIdentifier : BasicCommandExecution, IAtomicCommand
    {
        private Catalogue _catalogue;
        private ExtractionInformation[] _extractionInformations;
        private ExtractionInformation[] _alreadyMarked;

        public ExecuteCommandSetExtractionIdentifier(IBasicActivateItems activator,Catalogue catalogue):base(activator)
        {
            _catalogue = catalogue;

            _catalogue.ClearAllInjections();

            _extractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            if(_extractionInformations.Length == 0)
                SetImpossible("Catalogue does not have any extractable columns");

            _alreadyMarked = _extractionInformations.Where(ei => ei.IsExtractionIdentifier).ToArray();
        }

        public override string GetCommandName()
        {
            if(_alreadyMarked.Length == 0)
                return base.GetCommandName();

            return base.GetCommandName() + "(" + string.Join(",", _alreadyMarked.Select(e=>e.GetRuntimeName())) + ")";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort,OverlayKind.Key);
        }

        public override string GetCommandHelp()
        {
            return "Change which column(s) contain the patient id / linkage column e.g. CHI";
        }

        public override void Execute()
        {
            base.Execute();

            string initialSearchText = _alreadyMarked.Length == 1 ? _alreadyMarked[0].GetRuntimeName():null;

            if (SelectMany(_extractionInformations, out ExtractionInformation[] selected, initialSearchText))
            {
                if(selected.Length == 0)
                    if(!YesNo("Do you want to clear the Extraction Identifier?", "Clear Extraction Identifier?"))
                        return;

                if (selected.Length > 1)
                    if (!YesNo("Are you sure you want multiple linkable extraction identifier columns (most datasets only have 1 person ID column in them)?", "Multiple IsExtractionIdentifier columns?"))
                        return;

                foreach(var ei in _extractionInformations)
                {
                    bool newValue = selected.Contains(ei);

                    if(ei.IsExtractionIdentifier != newValue)
                    {
                        ei.IsExtractionIdentifier = newValue;
                        ei.SaveToDatabase();
                    }
                }


                Publish(_catalogue);
            }
        }
    }
}