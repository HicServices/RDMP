// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Startup.Events;

namespace Rdmp.Core.Databases;

/// <summary>
///     Finds <see cref="IPatcher" /> implementations
/// </summary>
public class PatcherManager
{
    /// <summary>
    ///     All patchers that are not plugins (<see cref="PluginPatcher" />) or the main rdmp patchers (
    ///     <see cref="CataloguePatcher" />)
    /// </summary>
    public IReadOnlyCollection<IPatcher> Tier2Patchers;

    /// <summary>
    ///     Creates a new instance populated with the default <see cref="IPatcher" /> types (DQE, Logging etc).
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

    public IEnumerable<PluginPatcher> GetTier3Patchers(PluginPatcherFoundHandler events)
    {
        foreach (var patcherType in MEF.GetTypes<PluginPatcher>().Where(static type => type.IsPublic))
        {
            PluginPatcher instance = null;

            try
            {
                instance = (PluginPatcher)ObjectConstructor.Construct(patcherType);

                events?.Invoke(this,
                    new PluginPatcherFoundEventArgs(patcherType, instance, PluginPatcherStatus.Healthy));
            }
            catch (Exception e)
            {
                events?.Invoke(this,
                    new PluginPatcherFoundEventArgs(patcherType, null, PluginPatcherStatus.CouldNotConstruct, e));
            }

            if (instance != null)
                yield return instance;
        }
    }

    /// <summary>
    ///     Returns all Tier 2 and 3 patchers (that could be constructed)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IPatcher> GetAllPatchers()
    {
        return Tier2Patchers.Union(GetTier3Patchers(null));
    }
}