// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandExecuteLoadMetadata : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public LoadMetadata LoadMetadata{ get; set; }

        [ImportingConstructor]
        public ExecuteCommandExecuteLoadMetadata(IActivateItems activator,LoadMetadata loadMetadata)
            : base(activator)
        {
            LoadMetadata = loadMetadata;


        }
        public ExecuteCommandExecuteLoadMetadata(IActivateItems activator) : base(activator)
        {
            
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadMetadata);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            LoadMetadata = (LoadMetadata)target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "Run the data load configuration through RAW=>STAGING=>LIVE";
        }

        public override string GetCommandName()
        {
            return "Execute Load";
        }

        public override void Execute()
        {
            if (LoadMetadata == null)
                SetImpossible("You must choose a LoadMetadata.");

            base.Execute();
            Activator.WindowArranger.SetupEditLoadMetadata(this, LoadMetadata);
        }
    }
}