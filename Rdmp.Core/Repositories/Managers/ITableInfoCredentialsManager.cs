// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
/// Models the relationship between <see cref="TableInfo"/> and the <see cref="DataAccessCredentials"/> (if any) that can be used to reach them.
/// </summary>
public interface ITableInfoCredentialsManager
{
    /// <summary>
    /// Declares that the given <paramref name="tableInfo"/> can be accessed using the <paramref name="credentials"/> (username / encrypted password) under the
    /// usage <paramref name="context"/>
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="tableInfo"></param>
    /// <param name="context"></param>
    void CreateLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context);

    /// <summary>
    /// Removes the right to use passed <paramref name="credentials"/> to access the <paramref name="tableInfo"/> under the <paramref name="context"/>
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="tableInfo"></param>
    /// <param name="context"></param>
    void BreakLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context);

    /// <summary>
    /// Removes all rights to use the passed <paramref name="credentials"/> to access the <paramref name="tableInfo"/>
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="tableInfo"></param>
    void BreakAllLinksBetween(DataAccessCredentials credentials, ITableInfo tableInfo);

    /// <summary>
    ///  Answers the question, "what is the best credential (if any) to use under the given context"
    /// 
    /// <para>Tries to find a DataAccessCredentials for the supplied TableInfo.  For example you are trying to find a username/password to use with the TableInfo when performing
    /// a DataLoad, this method will first return any explicit usage allowances (if there is a credential licenced for use during DataLoad) if no such credentials exist
    /// it will then check for a credential which is licenced for Any usage (can be used for data load, data export etc) and return that else it will return null</para>
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    DataAccessCredentials GetCredentialsIfExistsFor(ITableInfo tableInfo, DataAccessContext context);

    /// <summary>
    /// Fetches all <see cref="DataAccessCredentials"/> (username and encrypted password) that can be used to access the <see cref="TableInfo"/> under any
    /// <see cref="DataAccessContext"/>)
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <returns></returns>
    Dictionary<DataAccessContext, DataAccessCredentials> GetCredentialsIfExistsFor(ITableInfo tableInfo);

    /// <summary>
    /// Returns all credential usage permissions for the given set of <paramref name="allTableInfos"/> and <paramref name="allCredentials"/>
    /// </summary>
    /// <param name="allCredentials"></param>
    /// <param name="allTableInfos"></param>
    /// <returns></returns>
    Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(
        DataAccessCredentials[] allCredentials, ITableInfo[] allTableInfos);

    /// <summary>
    /// Returns all the <see cref="TableInfo"/> that are allowed to use the given <paramref name="credentials"/>
    /// </summary>
    /// <param name="credentials"></param>
    /// <returns></returns>
    Dictionary<DataAccessContext, List<ITableInfo>> GetAllTablesUsingCredentials(DataAccessCredentials credentials);

    /// <summary>
    /// Returns the existing <see cref="DataAccessCredentials"/> if any which match the unencrypted <paramref name="username"/> and <paramref name="password"/> combination.  Throws
    /// if there are more than 1
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password);
}