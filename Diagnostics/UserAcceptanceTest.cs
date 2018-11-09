using System;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace Diagnostics
{
    public abstract class UserAcceptanceTest
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        
        protected UserAcceptanceTest(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
        }

        protected Catalogue FindTestCatalogue(ICheckNotifier notifier)
        {
            Catalogue[] catas = RepositoryLocator.CatalogueRepository.GetAllCatalogues()
                .Where(c => c.Name.Equals(UserAcceptanceTestEnvironment.CatalogueName)).ToArray();

            if (catas.Length != 1)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Found " + catas.Length + " Catalogues called " + UserAcceptanceTestEnvironment.CatalogueName, CheckResult.Fail, null));
                return null;
            }

            notifier.OnCheckPerformed(new CheckEventArgs("Found test dataset catalogue '" + catas[0] + "'", CheckResult.Success, null));

            return catas[0];
        }

        protected bool PreviewAndExecuteSQL(string task, string sql, DbConnection con, ICheckNotifier notifier)
        {
            try
            {
                UsefulStuff.ExecuteBatchNonQuery(sql, con,timeout: 60);
                notifier.OnCheckPerformed(new CheckEventArgs("Succeeded at task:" + task, CheckResult.Success, null));
                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed at task:" + task, CheckResult.Fail, e));
                return false;
            }
        }
        
        
        protected bool DeleteOldTableInfoCalled(string tableName, ICheckNotifier notifier)
        {
            //cleanup old remenant
            foreach (TableInfo oldTable in RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>().Where(t => t.GetRuntimeName().Equals(tableName)))
            {
                //delete preload discarded columns
                foreach (var preLoadDiscardedColumn in oldTable.PreLoadDiscardedColumns)
                    preLoadDiscardedColumn.DeleteInDatabase();

                oldTable.DeleteInDatabase();
            }

            return true;
        }
    }
}