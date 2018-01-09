using System;
using System.ComponentModel.Composition;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace RDMPStartup
{
    /// <summary>
    /// MEF discoverable version of IPluginRepositoryFinder
    /// </summary>
    [InheritedExport(typeof(IPluginRepositoryFinder))]
    public abstract class PluginRepositoryFinder :IPluginRepositoryFinder
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        protected PluginRepositoryFinder(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
        }

        public abstract IRepository GetRepositoryIfAny();

        public abstract Type GetRepositoryType();
    }
}