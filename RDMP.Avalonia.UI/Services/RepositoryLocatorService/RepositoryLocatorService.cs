using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDMP.Avalonia.UI.Services.RepositoryLocatorService;

public class RepositoryLocatorService : ICheckNotifier
{

    public RepositoryLocatorService() { }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        // throw new NotImplementedException();
        return false;
    }

    public void StartScan()
    {
        var finder = new UserSettingsRepositoryFinder();
        BootstrapService.BootstrapService._startup.RepositoryLocator = finder;
        BootstrapService.BootstrapService._startup.DoStartup(this);
    }


    public void StartScan(string catalogueConnectionString,string dataExportconnectionString)
    {

        BootstrapService.BootstrapService._startup.RepositoryLocator = new LinkedRepositoryProvider(catalogueConnectionString, dataExportconnectionString);//todo this is causing problems
        BootstrapService.BootstrapService._startup.RepositoryLocator.CatalogueRepository.TestConnection();
        BootstrapService.BootstrapService._startup.RepositoryLocator.DataExportRepository.TestConnection();
    }

}
