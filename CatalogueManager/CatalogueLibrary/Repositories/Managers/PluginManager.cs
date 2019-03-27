using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Extensions;

namespace CatalogueLibrary.Repositories.Managers
{
    public class PluginManager : IPluginManager
    {
        private readonly ICatalogueRepository _repository;

        public PluginManager(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        public Plugin[] GetCompatiblePlugins()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            var plugins = _repository.GetAllObjects<Plugin>().Where(p => VersionExtensions.IsCompatibleWith(p.PluginVersion, new Version(version), 3));
            var uniquePlugins = plugins.GroupBy(p => new { name = p.Name, ver = new Version(p.PluginVersion.Major, p.PluginVersion.Minor, p.PluginVersion.Build) })
                .ToDictionary(g => g.Key, p => p.OrderByDescending(pv => pv.PluginVersion).First());
            return uniquePlugins.Values.ToArray();
        }
    }
}