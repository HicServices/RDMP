// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Sharing.Dependency;
using Rdmp.Core.Validation.Dependency;

namespace Rdmp.Core.Startup;

/// <summary>
///     Records the location of the Catalogue and DataExport databases in which RDMP stores all configuration information
///     (what datasets there are, what extraction
///     projects there are, what IFilters are available etc - literally everything, just look at who inherits from
///     IMapsDirectlyToDatabaseTable!).
///     <para>See also UserSettingsRepositoryFinder</para>
/// </summary>
public class LinkedRepositoryProvider : RepositoryProvider
{
    private List<IPluginRepositoryFinder> _pluginRepositoryFinders;


    public LinkedRepositoryProvider(string catalogueConnectionString, string dataExportConnectionString)
    {
        try
        {
            CatalogueRepository = string.IsNullOrWhiteSpace(catalogueConnectionString)
                ? null
                : new CatalogueRepository(new SqlConnectionStringBuilder(catalogueConnectionString));
        }
        catch (SourceCodeNotFoundException ex)
        {
            throw new Exception("There was a problem with the Catalogue connection string", ex);
        }

        try
        {
            DataExportRepository = string.IsNullOrWhiteSpace(dataExportConnectionString)
                ? null
                : new DataExportRepository(new SqlConnectionStringBuilder(dataExportConnectionString),
                    CatalogueRepository);
        }
        catch (Exception)
        {
            DataExportRepository = null;
            throw;
        }

        if (CatalogueRepository != null)
            ConfigureObscureDependencies();
    }

    /// <summary>
    ///     Call once if the <see cref="CatalogueRepository" /> and <see cref="DataExportRepository" /> are new (not existing
    ///     already).  This will populate the
    ///     <see cref="TableRepository.ObscureDependencyFinder" /> with appropriate cross database runtime constraints.
    /// </summary>
    private void ConfigureObscureDependencies()
    {
        //get the catalogues obscure dependency finder
        var finder = (CatalogueObscureDependencyFinder)CatalogueRepository.ObscureDependencyFinder;

        finder.AddOtherDependencyFinderIfNotExists<BetweenCatalogueAndDataExportObscureDependencyFinder>(this);
        finder.AddOtherDependencyFinderIfNotExists<ValidationXMLObscureDependencyFinder>(this);
        finder.AddOtherDependencyFinderIfNotExists<ObjectSharingObscureDependencyFinder>(this);

        if (DataExportRepository == null)
            return;

        if (DataExportRepository.ObscureDependencyFinder == null)
            DataExportRepository.ObscureDependencyFinder = new ObjectSharingObscureDependencyFinder(this);
        else if (DataExportRepository.ObscureDependencyFinder is not ObjectSharingObscureDependencyFinder)
            throw new Exception(
                "Expected DataExportRepository.ObscureDependencyFinder to be an ObjectSharingObscureDependencyFinder");
    }

    protected override IRepository GetRepository(string s)
    {
        LoadPluginRepositoryFindersIfNotLoaded();

        foreach (var repoFinder in _pluginRepositoryFinders)
            if (repoFinder.GetRepositoryType().FullName.Equals(s))
            {
                var toReturn = repoFinder.GetRepositoryIfAny() ?? throw new NotSupportedException(
                    $"IPluginRepositoryFinder '{repoFinder}' said that it was the correct repository finder for repository of type '{s}' but it was unable to find an existing repository instance (GetRepositoryIfAny returned null)");
                return toReturn;
            }


        return base.GetRepository(s);
    }

    protected virtual void LoadPluginRepositoryFindersIfNotLoaded()
    {
        if (_pluginRepositoryFinders != null)
            return;

        _pluginRepositoryFinders = new List<IPluginRepositoryFinder>();

        //it's a plugin?
        foreach (var type in MEF.GetTypes<IPluginRepositoryFinder>())
            _pluginRepositoryFinders.Add((IPluginRepositoryFinder)ObjectConstructor.Construct(type, this));
    }

    public override IEnumerable<IRepository> GetAllRepositories()
    {
        LoadPluginRepositoryFindersIfNotLoaded();

        yield return CatalogueRepository;

        if (DataExportRepository != null)
            yield return DataExportRepository;

        foreach (var r in _pluginRepositoryFinders.Select(p => p.GetRepositoryIfAny()).Where(r => r != null))
            yield return r;
    }
}