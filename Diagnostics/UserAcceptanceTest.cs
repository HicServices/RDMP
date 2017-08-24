using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.SqlDialogs;

namespace Diagnostics
{
    public abstract class UserAcceptanceTest
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        public bool SilentRunning { get; set; }

        protected UserAcceptanceTest(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {


            RepositoryLocator = repositoryLocator;
            CleanUp();
        }

        private void CleanUp()
        {
            if(RepositoryLocator.DataExportRepository != null)
                foreach (var extractableDataSet in RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableDataSet>().Where(e => e.Catalogue_ID == null))
                    extractableDataSet.DeleteInDatabase();
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
            DialogResult dialogResult;

            if (!SilentRunning)
            {
                var preview = new SQLPreviewWindow("Confirm happiness with SQL",
                    "The following SQL is about to be executed (In order to achieve task:'" + task + "')", sql);
                dialogResult = preview.ShowDialog();

                if (preview.YesToAll)
                    SilentRunning = true;
            }
            else
                dialogResult = DialogResult.OK;

            if (dialogResult != DialogResult.OK)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("User cancelled execution of some SQL, this is likely to break future steps but proceeding anyway ", CheckResult.Warning, null));
                return false;
            }
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
                if (!SilentRunning)
                    if(notifier.OnCheckPerformed(new CheckEventArgs("Found old copy of TableInfo for table " + oldTable.Name,CheckResult.Fail, null, "Delete and reimport it?")) == false)
                        return false;

                //delete preload discarded columns
                foreach (var preLoadDiscardedColumn in oldTable.PreLoadDiscardedColumns)
                    preLoadDiscardedColumn.DeleteInDatabase();

                oldTable.DeleteInDatabase();
            }

            return true;
        }
    }
}