// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace Diagnostics
{
    public class CohortConfigurationChecker : ICheckable
    {
        private readonly IDataExportRepository _repository;

        public CohortConfigurationChecker(IDataExportRepository repository)
        {
            _repository = repository;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                var cohortTables = _repository.GetAllObjects<ExternalCohortTable>().ToArray();
                if (!cohortTables.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("There are no ExternalCohortTables defined in DataExportManager",
                        CheckResult.Fail));

                foreach (ExternalCohortTable external in cohortTables)
                    external.Check(notifier);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Exception encountered when trying to perform checks", CheckResult.Fail, e));
            }

        }

       
    }
}
