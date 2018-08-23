using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandUseCredentialsToAccessTableInfoData : BasicUICommandExecution
    {
        private readonly CatalogueRepository _catalogueRepository;
        private readonly DataAccessCredentials _credentials;
        private readonly TableInfo _tableInfo;

        public ExecuteCommandUseCredentialsToAccessTableInfoData(IActivateItems activator,DataAccessCredentialsCommand sourceDataAccessCredentialsCommand, TableInfo targetTableInfo) : base(activator)
        {
            _credentials = sourceDataAccessCredentialsCommand.DataAccessCredentials;
            _catalogueRepository = _credentials.Repository as CatalogueRepository;

            _tableInfo = targetTableInfo;
            
            if(sourceDataAccessCredentialsCommand.CurrentUsage[DataAccessContext.Any].Contains(targetTableInfo))
                SetImpossible(sourceDataAccessCredentialsCommand.DataAccessCredentials + " is already used to access " + targetTableInfo + " under Any context");
        }

        public override void Execute()
        {
            base.Execute();

            _catalogueRepository.TableInfoToCredentialsLinker.CreateLinkBetween(_credentials,_tableInfo,DataAccessContext.Any);
            Publish(_tableInfo);
        }
    }
}