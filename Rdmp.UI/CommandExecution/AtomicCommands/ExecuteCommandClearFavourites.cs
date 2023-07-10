// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandClearFavourites : BasicUICommandExecution
{
    public ExecuteCommandClearFavourites(IActivateItems activator) : base(activator)
    {
        if(!Activator.FavouritesProvider.CurrentFavourites.Any())
            SetImpossible("You do not have any Favourites");
    }

    public override void Execute()
    {
        base.Execute();

        if (YesNo($"Delete '{Activator.FavouritesProvider.CurrentFavourites.Count}' Favourites",
                "Clear Favourites"))
        {
            var first = Activator.FavouritesProvider.CurrentFavourites.First();

            foreach (var f in Activator.FavouritesProvider.CurrentFavourites)
                f.DeleteInDatabase();

            //now that we have deleted them it is definitely not possible anymore
            SetImpossible("You do not have any Favourites");

            Activator.FavouritesProvider.CurrentFavourites.Clear();

            Publish(first);

        }
    }
}