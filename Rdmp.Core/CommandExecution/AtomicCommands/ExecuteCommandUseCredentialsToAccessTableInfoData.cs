// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandUseCredentialsToAccessTableInfoData : BasicCommandExecution
{
    private readonly DataAccessCredentials _credentials;
    private readonly TableInfo _tableInfo;
    private readonly DataAccessCredentials[] _available;

    [UseWithObjectConstructor]
    public ExecuteCommandUseCredentialsToAccessTableInfoData(IBasicActivateItems activator,
        DataAccessCredentials credentials, TableInfo targetTableInfo) : base(activator)
    {
        _credentials = credentials;

        _tableInfo = targetTableInfo;
            
        if(_credentials == null)
        {
            _available = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();

            if(_available.Length == 0)
                SetImpossible("There are no credentials configured");
        }
        else
        {
            var usage = _credentials.GetAllTableInfosThatUseThis();
                
            if(usage[DataAccessContext.Any].Contains(targetTableInfo))
                SetImpossible($"{_credentials} is already used to access {targetTableInfo} under Any context");
        }
    }

    public override void Execute()
    {
        base.Execute();
            
        var creds = _credentials ?? (DataAccessCredentials)BasicActivator.SelectOne("Select Credentials",_available);

        if(creds == null)
            return;

        BasicActivator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager.CreateLinkBetween(creds,_tableInfo,DataAccessContext.Any);
        Publish(_tableInfo);
    }
}