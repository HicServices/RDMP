// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
/// Helper for standardising access to properties on a class which reference a Pipeline.  Because many classes reference Pipelines and some reference multiple Pipelines
/// we use this class to abstract that away.  For example the CacheProgress constructor says to use "Pipeline_ID" int property.
/// 
/// <para>Currently used primarily by PipelineSelectionUIFactory </para>
/// </summary>
public class PipelineUser : IPipelineUser
{
    private readonly PropertyInfo _property;
    private ICatalogueRepository _catalogueRepository;

    /// <summary>
    /// The object which references a <see cref="Pipeline"/> for which you want the user to be able to change selected instance.
    /// </summary>
    public DatabaseEntity User { get; private set; }

    /// <inheritdoc/>
    public PipelineGetter Getter { get; private set; }

    /// <inheritdoc/>
    public PipelineSetter Setter { get; private set; }


    /// <summary>
    /// Declares that the given <paramref name="property"/> (which must be nullable int) stores the ID (or null) of a <see cref="Pipeline"/> declared
    /// in the RDMP platform databases.  The property must belong to <paramref name="user"/>
    /// </summary>
    /// <param name="property"></param>
    /// <param name="user"></param>
    /// <param name="repository"></param>
    public PipelineUser(PropertyInfo property, DatabaseEntity user, ICatalogueRepository repository = null)
    {
        _property = property;
        User = user;

        if (property.PropertyType != typeof(int?))
            throw new NotSupportedException($"Property {property} must be of PropertyType nullable int");

        //if user passed in an explicit one
        _catalogueRepository = repository;

        //otherwise get it from the user
        if (_catalogueRepository == null)
        {
            _catalogueRepository = User.Repository as ICatalogueRepository ??
                                   throw new Exception(
                                       "User does not have a Repository! how can it be a DatabaseEntity!");

            if (User.Repository is IDataExportRepository dataExportRepo)
                _catalogueRepository = dataExportRepo.CatalogueRepository;

            if (_catalogueRepository == null)
                throw new Exception(
                    $"Repository of Host '{User}' was not an ICatalogueRepository or a IDataExportRepository.  user came from a Repository called '{user.Repository.GetType().Name}' in this case you will need to specify the ICatalogueRepository property to this method so we know where to fetch Pipelines from");
        }

        Getter = Get;
        Setter = Set;
    }

    /// <summary>
    /// Gets a <see cref="PipelineUser"/> targeting <see cref="CacheProgress.Pipeline_ID"/>
    /// </summary>
    /// <param name="cacheProgress"></param>
    public PipelineUser(CacheProgress cacheProgress) : this(typeof(CacheProgress).GetProperty("Pipeline_ID"),
        cacheProgress)
    {
    }

    private Pipeline Get()
    {
        var id = (int?)_property.GetValue(User);

        return id == null ? null : _catalogueRepository.GetObjectByID<Pipeline>(id.Value);
    }

    private void Set(Pipeline newPipelineOrNull)
    {
        if (newPipelineOrNull != null)
            _property.SetValue(User, newPipelineOrNull.ID);
        else
            _property.SetValue(User, null);

        User.SaveToDatabase();
    }
}

//for classes which have a Pipeline_ID column but btw the reason we don't just have IPipelineHost as an interface is because you can have 2+ Pipeline IDs e.g. ExtractionConfiguration which has a Default and a Refresh
/// <summary>
/// Delegate for storing a new value of <see cref="Pipeline"/> into a class
/// </summary>
/// <param name="newPipelineOrNull"></param>
public delegate void PipelineSetter(Pipeline newPipelineOrNull);

/// <summary>
/// Delegate for retrieving the currently set <see cref="Pipeline"/> of a class
/// </summary>
public delegate Pipeline PipelineGetter();