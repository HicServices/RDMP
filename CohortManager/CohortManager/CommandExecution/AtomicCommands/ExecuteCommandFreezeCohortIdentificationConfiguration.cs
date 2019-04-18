// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CohortManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandFreezeCohortIdentificationConfiguration : BasicUICommandExecution,IAtomicCommand
    {
        private readonly CohortIdentificationConfiguration _cic;
        private readonly bool _desiredFreezeState;

        public ExecuteCommandFreezeCohortIdentificationConfiguration(IActivateItems activator, CohortIdentificationConfiguration cic, bool desiredFreezeState):base(activator)
        {
            _cic = cic;
            _desiredFreezeState = desiredFreezeState;
        }

        public override string GetCommandName()
        {
            return _desiredFreezeState ? "Freeze Configuration" : "Unfreeze Configuration";
        }

        public override void Execute()
        {
            base.Execute();

            if (_desiredFreezeState)
                _cic.Freeze();
            else
                _cic.Unfreeze();

            Publish(_cic);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.FrozenCohortIdentificationConfiguration;
        }
    }
}