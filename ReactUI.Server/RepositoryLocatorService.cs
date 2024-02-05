namespace ReactUI.Server;
using Rdmp.Core.Databases;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
        RDMPInitialiser._startup.RepositoryLocator = finder;
        RDMPInitialiser._startup.DoStartup(this);
    }


    public void StartScan(string catalogueConnectionString, string dataExportconnectionString)
    {

        RDMPInitialiser._startup.RepositoryLocator = new LinkedRepositoryProvider(catalogueConnectionString, dataExportconnectionString);//todo this is causing problems
        RDMPInitialiser._startup.RepositoryLocator.CatalogueRepository.TestConnection();
        RDMPInitialiser._startup.RepositoryLocator.DataExportRepository.TestConnection();
        RDMPInitialiser._startup.DoStartup(this);
    }

}
