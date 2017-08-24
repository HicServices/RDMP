using CatalogueLibrary.Repositories;
using RDMPStartup;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public interface IRepositoryUser
    {
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
    }
}