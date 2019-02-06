// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandSetDataAccessContextForCredentials : BasicUICommandExecution, IAtomicCommand
    {
        private DataAccessCredentialUsageNode _node;
        private readonly DataAccessContext _newContext;

        public ExecuteCommandSetDataAccessContextForCredentials(IActivateItems activator, DataAccessCredentialUsageNode node, DataAccessContext newContext, Dictionary<DataAccessContext, DataAccessCredentials> existingCredentials): base(activator)
        {
            _node = node;
            _newContext = newContext;

            //if context is same as before
            if(newContext == node.Context)
            {
                SetImpossible("This is the current usage context declared");
                return;
            }
            
            //if theres already another credentials for that context (other than this one)
            if (existingCredentials.ContainsKey(newContext))
                SetImpossible("DataAccessCredentials '" + existingCredentials[newContext] + "' are used for accessing table under context " + newContext);
        }

        public override string GetCommandHelp()
        {
            return "Changes which contexts the credentials can be used under e.g. DataLoad only";
        }

        public override string GetCommandName()
        {
            return _newContext.ToString();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            base.Execute();
            Activator.RepositoryLocator.CatalogueRepository.TableInfoToCredentialsLinker.SetContextFor(_node, _newContext);
            Publish(_node.TableInfo);
        }
    }
}