using System;
using CatalogueLibrary.Repositories;

namespace CatalogueManager.PluginDbInit
{
    public sealed class DbInitCommands
    {
        public string CommandTitle { get; private set; }

        public Action<IRDMPPlatformRepositoryServiceLocator> Command { get; private set; }

        public DbInitCommands(string title, Action<IRDMPPlatformRepositoryServiceLocator> command)
        {
            CommandTitle = title;
            Command = command;
        }
    }
}