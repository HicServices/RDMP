// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.AggregationUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.ObjectVisualisation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Dependencies.Models;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandFindUsages : BasicUICommandExecution,IAtomicCommand
    {
        private Gatherer _gatherer;
        private DatabaseEntity _o;

        public ExecuteCommandFindUsages(IActivateItems activator, DatabaseEntity o) : base(activator)
        {
            _gatherer = new Gatherer(activator.RepositoryLocator);
            if(!_gatherer.CanGatherDependencies(o))
                SetImpossible("Object Type " + o.GetType() + " is not compatible with Gatherer");
            _o = o;
        }

        public override void Execute()
        {
            base.Execute();

            var dependencies = _gatherer.GatherDependencies(_o);

            var cmd = new ExecuteCommandViewDependencies(dependencies, Activator.GetLazyCatalogueObjectVisualisation());
            cmd.Execute();
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}