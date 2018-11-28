using System;
using System.ComponentModel.Composition;
using System.Reflection;
using CatalogueLibrary.Repositories;

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


        /// <inheritdoc/>
        public abstract PluginRepository GetRepositoryIfAny();

        /// <inheritdoc/>
        public abstract Type GetRepositoryType();
    }
}