// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditExistingCatalogue : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public Catalogue Catalogue { get; set; }

        [UseWithObjectConstructor]
        public ExecuteCommandEditExistingCatalogue(IActivateItems activator,Catalogue catalogue)
            : base(activator)
        {
            Catalogue = catalogue;
        }


        public ExecuteCommandEditExistingCatalogue(IActivateItems activator) : base(activator)
        {
            
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            Catalogue = (Catalogue) target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "This will take you to the Catalogues list and allow you to Edit the Catalogue and Dataset table metadata." +
                   "\r\n" +
                   "You must choose a Catalogue from the list before proceeding.";
        }

        public override void Execute()
        {
            base.Execute();

            var c = Catalogue ?? SelectOne<Catalogue>(BasicActivator.RepositoryLocator.CatalogueRepository);

            if(c == null)
                return;

            Activator.WindowArranger.SetupEditAnything(this, c);
        }
    }
}