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
