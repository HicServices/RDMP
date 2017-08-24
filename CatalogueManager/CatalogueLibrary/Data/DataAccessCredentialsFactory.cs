using CatalogueLibrary.Repositories;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    public class DataAccessCredentialsFactory
    {
        private readonly CatalogueRepository _cataRepository;

        public DataAccessCredentialsFactory(CatalogueRepository cataRepository)
        {
            _cataRepository = cataRepository;
        }

        /// <summary>
        /// Ensures that the passed username/password combination are used to access the TableInfo under the provided context.  This will either create a new DataAccessCredentials 
        /// or wire up the TableInfo with a new usage permission to an existing one (if the same username/password combination already exists).
        /// </summary>
        /// <param name="tableInfoCreated"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="usageContext"></param>
        public DataAccessCredentials Create(TableInfo tableInfoCreated, string username, string password, DataAccessContext usageContext)
        {
            DataAccessCredentials credentialsToAssociate = _cataRepository.TableInfoToCredentialsLinker.GetCredentialByUsernameAndPasswordIfExists(username, password);

            if (credentialsToAssociate == null)
            {
                //create one
                credentialsToAssociate = new DataAccessCredentials(_cataRepository);
                credentialsToAssociate.Username = username;
                credentialsToAssociate.Password = password;
                credentialsToAssociate.SaveToDatabase();
            }

            _cataRepository.TableInfoToCredentialsLinker.CreateLinkBetween(credentialsToAssociate, tableInfoCreated,usageContext);

            return credentialsToAssociate;
        }
    }
}