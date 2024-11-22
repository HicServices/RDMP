// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandSetDataAccessContextForCredentials : BasicCommandExecution
{
    private DataAccessCredentialUsageNode _node;
    private readonly DataAccessContext _newContext;

    public ExecuteCommandSetDataAccessContextForCredentials(IBasicActivateItems activator,
        DataAccessCredentialUsageNode node, DataAccessContext newContext,
        Dictionary<DataAccessContext, DataAccessCredentials> existingCredentials) : base(activator)
    {
        _node = node;
        _newContext = newContext;

        //if context is same as before
        if (newContext == node.Context)
        {
            SetImpossible("This is the current usage context declared");
            return;
        }

        //if there's already another credential for that context (other than this one)
        if (existingCredentials.TryGetValue(newContext, out var existingCredential))
            SetImpossible(
                $"DataAccessCredentials '{existingCredential}' are used for accessing table under context {newContext}");
    }

    public override string GetCommandHelp() =>
        "Changes which contexts the credentials can be used under e.g. DataLoad only";

    public override string GetCommandName() => _newContext.ToString();

    public override void Execute()
    {
        base.Execute();

        //don't bother if it is already at that context
        if (_node.Context == _newContext)
            return;

        var linker = BasicActivator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager;

        linker.BreakLinkBetween(_node.Credentials, _node.TableInfo, _node.Context);
        linker.CreateLinkBetween(_node.Credentials, _node.TableInfo, _newContext);

        Publish(_node.TableInfo);
    }
}