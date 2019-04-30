using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Startup.Events;

namespace Rdmp.Core.Databases
{
    /// <summary>
    /// Finds <see cref="IPatcher"/> implementations
    /// </summary>
    public class PatcherManager
    {
        /// <summary>
        /// All patchers that are not plugins (<see cref="PluginPatcher"/>) or the main rdmp patchers (<see cref="CataloguePatcher"/>)
        /// </summary>
        public IReadOnlyCollection<IPatcher> Tier2Patchers;

        /// <summary>
        /// Creates a new instance populated with the default <see cref="IPatcher"/> types (DQE, Logging etc).
        /// </summary>
        public PatcherManager()
        {
            //DQE
            Tier2Patchers = new IPatcher[]
            {
                new DataQualityEnginePatcher(),
                new LoggingDatabasePatcher(),
                new ANOStorePatcher(),
                new IdentifierDumpDatabasePatcher(),
                new QueryCachingPatcher()
            };
        }

        public IEnumerable<PluginPatcher> GetTier3Patchers(MEF mef,PluginPatcherFoundHandler events)
        {
            ObjectConstructor constructor = new ObjectConstructor();
            
            foreach (Type patcherType in mef.GetTypes<PluginPatcher>().Where(type => type.IsPublic))
            {
                PluginPatcher instance = null;
                
                try
                {
                    instance = (PluginPatcher)constructor.Construct(patcherType);

                    events?.Invoke(this, new PluginPatcherFoundEventArgs(patcherType, instance, PluginPatcherStatus.Healthy));
                }
                catch (Exception e)
                {
                    events?.Invoke(this, new PluginPatcherFoundEventArgs(patcherType, null, PluginPatcherStatus.CouldNotConstruct, e));
                }

                if(instance != null)
                    yield return instance;
            }
        }

        /// <summary>
        /// Returns all Tier 2 and 3 patchers (that could be constructed)
        /// </summary>
        /// <param name="mef"></param>
        /// <returns></returns>
        public IEnumerable<IPatcher> GetAllPatchers(MEF mef)
        {
            return Tier2Patchers.Union(GetTier3Patchers(mef,null));
        }
    }
}