using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandUseCredentialsToAccessTableInfoData : BasicCommandExecution
    {
        private readonly CatalogueRepository _catalogueRepository;
        private readonly DataAccessCredentials _credentials;
        private readonly IActivateItems _activator;
        private readonly TableInfo _tableInfo;

        public ExecuteCommandUseCredentialsToAccessTableInfoData(IActivateItems activator,DataAccessCredentialsCommand sourceDataAccessCredentialsCommand, TableInfo targetTableInfo)
        {
            _credentials = sourceDataAccessCredentialsCommand.DataAccessCredentials;
            _catalogueRepository = _credentials.Repository as CatalogueRepository;

            _activator = activator;
            _tableInfo = targetTableInfo;
            
            if(sourceDataAccessCredentialsCommand.CurrentUsage[DataAccessContext.Any].Contains(targetTableInfo))
                SetImpossible(sourceDataAccessCredentialsCommand.DataAccessCredentials + " is already used to access " + targetTableInfo + " under Any context");
        }

        public override void Execute()
        {
            base.Execute();

            _catalogueRepository.TableInfoToCredentialsLinker.CreateLinkBetween(_credentials,_tableInfo,DataAccessContext.Any);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_tableInfo));
        }
    }
}