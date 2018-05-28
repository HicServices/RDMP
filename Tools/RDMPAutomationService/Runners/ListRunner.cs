using System;
using System.Text.RegularExpressions;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    class ListRunner :IRunner
    {
        private ListOptions _options;

        public ListRunner(ListOptions options)
        {
            _options = options;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            var dbType = repositoryLocator.CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(_options.Type);

            if(!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(dbType))
                throw new NotSupportedException("Only Types derrived from IMapsDirectlyToDatabaseTable can be listed");

            if (repositoryLocator.CatalogueRepository.SupportsObjectType(dbType))
                ListObjects(repositoryLocator.CatalogueRepository, dbType);
            else if (repositoryLocator.DataExportRepository.SupportsObjectType(dbType))
                ListObjects(repositoryLocator.DataExportRepository, dbType);
            else
                throw new NotSupportedException("No IRepository owned up to supporting Type '" + dbType.FullName + "'");

            return 0;
        }

        private void ListObjects(IRepository repository, Type dbType)
        {
            var regex = new Regex(_options.Pattern);
            Console.WriteLine(string.Format("[ID]\t- Name"));

            if (_options.ID != null)
                Show(repository.GetObjectByID(dbType, _options.ID.Value));
            else
                foreach (IMapsDirectlyToDatabaseTable o in repository.GetAllObjects(dbType))
                    if (regex.IsMatch(o.ToString()))
                        Show(o);
        }

        private void Show(IMapsDirectlyToDatabaseTable o)
        {
            Console.WriteLine("[{0}]\t - {1}", o.ID, o);
        }
    }
}
