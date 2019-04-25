// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.CommandExecution.AtomicCommands
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

            _catalogueRepository.TableInfoCredentialsManager.CreateLinkBetween(_credentials,_tableInfo,DataAccessContext.Any);
            Publish(_tableInfo);
        }
    }
}