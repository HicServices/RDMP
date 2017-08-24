using System;
using MapsDirectlyToDatabaseTable;

namespace RDMPStartup
{
    public interface IPluginRepositoryFinder
    {
        IRepository GetRepositoryIfAny();
        Type GetRepositoryType();
    }
}