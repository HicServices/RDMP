// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Tests.CommandExecution;

internal class ExecuteCommandClearFavouritesTests : UITests
{
    [Test]
    [UITimeout(50000)]
    public void Test_NoFavourites()
    {
        var cmd = new ExecuteCommandClearFavourites(ItemActivator);

        Assert.That(cmd.IsImpossible);
        Assert.That(cmd.ReasonCommandImpossible, Is.EqualTo("You do not have any Favourites").IgnoreCase);

        var myFavCatalogue = WhenIHaveA<Catalogue>();

        ItemActivator.FavouritesProvider.AddFavourite(this, myFavCatalogue);

        cmd = new ExecuteCommandClearFavourites(ItemActivator);
        Assert.That(cmd.IsImpossible, Is.False);
    }


    [Test]
    [UITimeout(50000)]
    public void Test_ClearFavourites()
    {
        var myFavCatalogue = WhenIHaveA<Catalogue>();
        var mProject = WhenIHaveA<Project>();

        ItemActivator.FavouritesProvider.AddFavourite(this, myFavCatalogue);
        ItemActivator.FavouritesProvider.AddFavourite(this, mProject);

        Assert.That(ItemActivator.FavouritesProvider.CurrentFavourites, Has.Count.EqualTo(2));

        //when we say no to deleting them
        ItemActivator.YesNoResponse = false;

        var cmd = new ExecuteCommandClearFavourites(ItemActivator);
        cmd.Execute();

        //they should not be deleted!
        Assert.That(ItemActivator.FavouritesProvider.CurrentFavourites, Has.Count.EqualTo(2));

        //when we say yes to deleting them
        ItemActivator.YesNoResponse = true;
        cmd.Execute();

        //they should not be deleted
        Assert.That(ItemActivator.FavouritesProvider.CurrentFavourites, Is.Empty);
    }
}