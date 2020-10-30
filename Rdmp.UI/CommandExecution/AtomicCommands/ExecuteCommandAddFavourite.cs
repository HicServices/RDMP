// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.NavigateTo;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddFavourite : BasicUICommandExecution
    {
        private DatabaseEntity _databaseEntity;

        public ExecuteCommandAddFavourite(IActivateItems activator) : base(activator)
        {
        }

        public ExecuteCommandAddFavourite(IActivateItems activator, DatabaseEntity databaseEntity) : this(activator)
        {
            _databaseEntity = databaseEntity;
        }

        public override string GetCommandName()
        {
            if(_databaseEntity == null)
                return base.GetCommandName();

            return Activator.FavouritesProvider.IsFavourite(_databaseEntity) ? "UnFavourite" : "Favourite";
        }

        public override void Execute()
        {
            base.Execute();

            if(_databaseEntity != null)
            {
                if(Activator.FavouritesProvider.IsFavourite(_databaseEntity))
                    Activator.FavouritesProvider.RemoveFavourite(this,_databaseEntity);
                else
                    Activator.FavouritesProvider.AddFavourite(this,_databaseEntity);
            }
            else
            {
                NavigateToObjectUI navigate = new NavigateToObjectUI(Activator){Text = "Add Favourite"};
                navigate.CompletionAction = (a) =>
                {
                    if (Activator.FavouritesProvider.IsFavourite(a))
                        Show($"'{a}' is already a Favourite");
                    else
                        Activator.FavouritesProvider.AddFavourite(this, a);
                };
                navigate.Show();
            }

        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            if(_databaseEntity != null && Activator.FavouritesProvider.IsFavourite(_databaseEntity))
                return CatalogueIcons.StarHollow;

            return iconProvider.GetImage(RDMPConcept.Favourite,OverlayKind.Add);
        }
    }
}