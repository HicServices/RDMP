// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Helper Factory for creating DataAccessCredentials.  This class exists solely to prevent duplication in
///     DataAccessCredentials being created for newly imported
///     TableInfos where the username/password/server are the same as an existing DataAccessCredentials.
/// </summary>
public class DataAccessCredentialsFactory
{
    private readonly ICatalogueRepository _cataRepository;

    /// <summary>
    ///     Creates a new <see cref="DataAccessCredentialsFactory" /> for creating <see cref="DataAccessCredentials" /> which
    ///     will be stored in the database provided (<paramref name="cataRepository" />)
    /// </summary>
    /// <param name="cataRepository"></param>
    public DataAccessCredentialsFactory(ICatalogueRepository cataRepository)
    {
        _cataRepository = cataRepository;
    }

    /// <summary>
    ///     Ensures that the passed username/password combination are used to access the TableInfo under the provided context.
    ///     This will either create a new DataAccessCredentials
    ///     or wire up the TableInfo with a new usage permission to an existing one (if the same username/password combination
    ///     already exists).
    /// </summary>
    /// <param name="tableInfoCreated"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="usageContext"></param>
    public DataAccessCredentials Create(ITableInfo tableInfoCreated, string username, string password,
        DataAccessContext usageContext)
    {
        var credentialsToAssociate =
            _cataRepository.TableInfoCredentialsManager.GetCredentialByUsernameAndPasswordIfExists(username, password);

        if (credentialsToAssociate == null)
        {
            //create one
            credentialsToAssociate = new DataAccessCredentials(_cataRepository)
            {
                Username = username,
                Password = password
            };
            credentialsToAssociate.SaveToDatabase();
        }

        _cataRepository.TableInfoCredentialsManager.CreateLinkBetween(credentialsToAssociate, tableInfoCreated,
            usageContext);

        return credentialsToAssociate;
    }
}