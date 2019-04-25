// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCloneExtractionConfiguration : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;

        public ExecuteCommandCloneExtractionConfiguration(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;

            if(!_extractionConfiguration.SelectedDataSets.Any())
                SetImpossible("ExtractionConfiguration does not have any selected datasets");
        }

        public override string GetCommandHelp()
        {
            return "Creates an exact copy of the Extraction Configuration including the cohort selection, all selected datasets, parameters, filter containers, filters etc";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.CloneExtractionConfiguration;
        }

        public override void Execute()
        {
            base.Execute();

            try
            {
                var clone = _extractionConfiguration.DeepCloneWithNewIDs();
                
                Publish((DatabaseEntity)clone.Project);
                Emphasise(clone,int.MaxValue);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}