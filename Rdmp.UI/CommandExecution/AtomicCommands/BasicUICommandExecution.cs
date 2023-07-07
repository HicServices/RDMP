// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.UI.ItemActivation;


namespace Rdmp.UI.CommandExecution.AtomicCommands;

public abstract class BasicUICommandExecution:BasicCommandExecution
{
    protected readonly IActivateItems Activator;

    protected BasicUICommandExecution(IActivateItems activator):base(activator)
    {
        Activator = activator;
    }
        
    protected static FileInfo SelectSaveFile(string filter)
    {
        var sfd = new SaveFileDialog
        {
            Filter = filter
        };
        if (sfd.ShowDialog() == DialogResult.OK)
            return new FileInfo(sfd.FileName);

        return null;
    }

    protected static FileInfo SelectOpenFile(string filter)
    {
        var ofd = new OpenFileDialog
        {
            Filter = filter
        };
        if (ofd.ShowDialog() == DialogResult.OK)
            return new FileInfo(ofd.FileName);

        return null;
    }

    internal void SetDefaultIfNotExists(ExternalDatabaseServer newServer, PermissableDefaults permissableDefault, bool askYesNo)
    {
        var defaults = Activator.RepositoryLocator.CatalogueRepository;

        var current = defaults.GetDefaultFor(permissableDefault);
            
        if(current == null)
            if(!askYesNo || YesNo($"Set as the default {permissableDefault} server?", "Set as default"))
                defaults.SetDefault(permissableDefault,newServer);
    }
}