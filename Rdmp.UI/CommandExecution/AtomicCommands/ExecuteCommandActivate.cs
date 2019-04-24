// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueManager.ItemActivation;
using Rdmp.Core.CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandActivate : BasicUICommandExecution,IAtomicCommand
    {
        private readonly object _o;
        
        public ExecuteCommandActivate(IActivateItems activator, object o) : base(activator)
        {    
            _o = o;

            var masquerader = _o as IMasqueradeAs;

            //if we have a masquerader and we cannot activate the masquerader, maybe we can activate what it is masquerading as?
            if (masquerader != null && !Activator.CommandExecutionFactory.CanActivate(masquerader))
                _o = masquerader.MasqueradingAs();

            if(!Activator.CommandExecutionFactory.CanActivate(_o))
                SetImpossible("Object cannot be Activated");
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            if (_o == null)
                return null;

            return iconProvider.GetImage(_o, OverlayKind.Edit);
        }

        public override string GetCommandName()
        {
            return "Edit";
        }

        public override void Execute()
        {
            base.Execute();

            Activator.CommandExecutionFactory.Activate(_o);
        }
    }
}