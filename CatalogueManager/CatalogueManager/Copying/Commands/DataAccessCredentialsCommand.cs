using System.Collections.Generic;
using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class DataAccessCredentialsCommand : ICommand
    {
        public DataAccessCredentials DataAccessCredentials { get; private set; }
        public Dictionary<DataAccessContext, List<TableInfo>> CurrentUsage { get; set; }

        public DataAccessCredentialsCommand(DataAccessCredentials dataAccessCredentials)
        {
            DataAccessCredentials = dataAccessCredentials;
            CurrentUsage = DataAccessCredentials.GetAllTableInfosThatUseThis();
        }

        

        public string GetSqlString()
        {
            return null;
        }
    }
}