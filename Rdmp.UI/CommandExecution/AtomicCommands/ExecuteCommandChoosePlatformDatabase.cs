// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.Startup;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.LocationsMenu;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandChoosePlatformDatabase : BasicCommandExecution
{
    private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

    public ExecuteCommandChoosePlatformDatabase(IActivateItems activator)
    {
        if (activator != null)
            Initialize(activator.RepositoryLocator);
    }

    public ExecuteCommandChoosePlatformDatabase(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        Initialize(repositoryLocator);
    }

    private void Initialize(IRDMPPlatformRepositoryServiceLocator locator)
    {
        _repositoryLocator = locator;
        if (_repositoryLocator is not UserSettingsRepositoryFinder)
            SetImpossible("Platform databases location is read-only (probably passed as command line parameter?).");
    }

    public override string GetCommandHelp() => "Change which RDMP platform metadata databases you are connected to";

    public override void Execute()
    {
        base.Execute();

        var dialog = new ChoosePlatformDatabasesUI(_repositoryLocator);
        dialog.ShowDialog();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.Database);
}