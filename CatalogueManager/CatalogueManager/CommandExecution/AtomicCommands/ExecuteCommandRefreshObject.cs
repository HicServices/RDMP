// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Rdmp.Core.CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;


namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRefreshObject:BasicUICommandExecution,IAtomicCommand
    {
        private readonly DatabaseEntity _databaseEntity;

        public ExecuteCommandRefreshObject(IActivateItems activator, DatabaseEntity databaseEntity) : base(activator)
        {
            _databaseEntity = databaseEntity;

            if(_databaseEntity == null)
                SetImpossible("No DatabaseEntity was specified");
        }

        public override void Execute()
        {
            base.Execute();

            _databaseEntity.RevertToDatabaseState();
            Publish(_databaseEntity);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.arrow_refresh;
        }
    }
}