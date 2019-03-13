using System.Collections.Generic;
using CatalogueLibrary.Nodes;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Models the relationship between <see cref="TableInfo"/> and the <see cref="DataAccessCredentials"/> (if any) that can be used to reach them.
    /// </summary>
    public interface ITableInfoToCredentialsLinker
    {
        /// <summary>
        /// Declares that the given <paramref name="tableInfo"/> can be accessed using the <paramref name="credentials"/> (username / encrypted password) under the 
        /// usage <paramref name="context"/> 
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="tableInfo"></param>
        /// <param name="context"></param>
        void CreateLinkBetween(DataAccessCredentials credentials, TableInfo tableInfo,DataAccessContext context);

        /// <summary>
        /// Removes the right to use passed <paramref name="credentials"/> to access the <paramref name="tableInfo"/> under the <paramref name="context"/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="tableInfo"></param>
        /// <param name="context"></param>
        void BreakLinkBetween(DataAccessCredentials credentials, TableInfo tableInfo, DataAccessContext context);

        /// <summary>
        /// Removes all rights to use the passed <paramref name="credentials"/> to access the <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="tableInfo"></param>
        void BreakAllLinksBetween(DataAccessCredentials credentials, TableInfo tableInfo);

        /// <summary>
        ///  Answers the question, "what is the best credential (if any) to use under the given context"
        /// 
        /// <para>Tries to find a DataAccessCredentials for the supplied TableInfo.  For example you are trying to find a username/pasword to use with the TableInfo when performing
        /// a DataLoad, this method will first return any explicit usage allowances (if there is a credential liscenced for use during DataLoad) if no such credentials exist 
        /// it will then check for a credential which is liscenced for Any usage (can be used for data load, data export etc) and return that else it will return null</para>
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        DataAccessCredentials GetCredentialsIfExistsFor(TableInfo tableInfo, DataAccessContext context);
        
        /// <summary>
        /// Fetches all <see cref="DataAccessCredentials"/> (username and encrypted password) that can be used to access the <see cref="TableInfo"/> under any
        /// <see cref="DataAccessContext"/>)
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        Dictionary<DataAccessContext,DataAccessCredentials> GetCredentialsIfExistsFor(TableInfo tableInfo);

        /// <summary>
        /// Returns all credential usage permissions for the given set of <paramref name="allTableInfos"/> and <paramref name="allCredentials"/>
        /// </summary>
        /// <param name="allCredentials"></param>
        /// <param name="allTableInfos"></param>
        /// <returns></returns>
        Dictionary<TableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(DataAccessCredentials[] allCredentials, TableInfo[] allTableInfos);

        /// <summary>
        /// Returns all the <see cref="TableInfo"/> that are allowed to use the given <paramref name="credentials"/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        Dictionary<DataAccessContext, List<TableInfo>> GetAllTablesUsingCredentials(DataAccessCredentials credentials);

        /// <summary>
        /// Returns the existing <see cref="DataAccessCredentials"/> if any which match the unencrypted <paramref name="username"/> and <paramref name="password"/> combination.  Throws
        /// if there are more than 1 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password);
    }
}