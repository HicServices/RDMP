// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRunChecksInPopupWindow : BasicUICommandExecution, IAtomicCommand
    {
        private ICheckable _checkable;

        public ExecuteCommandRunChecksInPopupWindow(IActivateItems activator, ICheckable checkable) : base(activator)
        {
            _checkable = checkable;
        }

        public override string GetCommandName()
        {
            return "Run Checks ("+ _checkable.GetType().Name +")";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Warning;
        }

        public override void Execute()
        {
            base.Execute();
            var p = new PopupChecksUI(GetCommandName(),false);
            p.StartChecking(_checkable);
        }
    }
}