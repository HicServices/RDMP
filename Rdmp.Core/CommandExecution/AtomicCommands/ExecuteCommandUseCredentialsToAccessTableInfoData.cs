// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandUseCredentialsToAccessTableInfoData : BasicCommandExecution
    {
        private readonly CatalogueRepository _catalogueRepository;
        private readonly DataAccessCredentials _credentials;
        private readonly TableInfo _tableInfo;

        [UseWithObjectConstructor]
        public ExecuteCommandUseCredentialsToAccessTableInfoData(IBasicActivateItems activator,
            DataAccessCredentials credentials, TableInfo targetTableInfo) : this(activator,
            new DataAccessCredentialsCombineable(credentials), targetTableInfo)
        {

        }

        public ExecuteCommandUseCredentialsToAccessTableInfoData(IBasicActivateItems activator,DataAccessCredentialsCombineable sourceDataAccessCredentialsCombineable, TableInfo targetTableInfo) : base(activator)
        {
            _credentials = sourceDataAccessCredentialsCombineable.DataAccessCredentials;
            _catalogueRepository = _credentials.Repository as CatalogueRepository;

            _tableInfo = targetTableInfo;
            
            if(sourceDataAccessCredentialsCombineable.CurrentUsage[DataAccessContext.Any].Contains(targetTableInfo))
                SetImpossible(sourceDataAccessCredentialsCombineable.DataAccessCredentials + " is already used to access " + targetTableInfo + " under Any context");
        }

        public override void Execute()
        {
            base.Execute();

            _catalogueRepository.TableInfoCredentialsManager.CreateLinkBetween(_credentials,_tableInfo,DataAccessContext.Any);
            Publish(_tableInfo);
        }
    }
}