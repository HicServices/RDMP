// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.Repositories;
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
            Catalogue[] catas = RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
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