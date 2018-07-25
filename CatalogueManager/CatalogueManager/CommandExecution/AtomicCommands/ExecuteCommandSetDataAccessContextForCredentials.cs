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