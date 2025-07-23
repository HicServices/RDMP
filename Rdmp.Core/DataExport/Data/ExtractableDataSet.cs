// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="IExtractableDataSet"/>
public class ExtractableDataSet : DatabaseEntity, IExtractableDataSet, IInjectKnown<ICatalogue>
{
    #region Database Properties

    private int _catalogue_ID;
    private bool _disableExtraction;
    private List<IProject> _projects;


    /// <inheritdoc/>
    public int Catalogue_ID
    {
        get => _catalogue_ID;
        set
        {
            ClearAllInjections();
            SetField(ref _catalogue_ID, value);
        }
    }

    /// <inheritdoc/>
    public bool DisableExtraction
    {
        get => _disableExtraction;
        set => SetField(ref _disableExtraction, value);
    }


    /// <inheritdoc/>
    [NoMappingToDatabase]
    public List<IProject> Projects
    {
        get
        {
            if (_projects != null) return _projects;
            var ids = Repository.GetAllObjectsWhere<ExtractableDataSetProject>("ExtractableDataSet_ID", this.ID).Select(edsp => edsp.Project_ID);
            Projects = Repository.GetAllObjectsInIDList<Project>(ids).Cast<IProject>().ToList();
            return _projects;

        }
        set => SetField(ref _projects, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    /// Returns all <see cref="IExtractionConfiguration"/> in which this dataset is one of the extracted datasets
    /// </summary>
    [NoMappingToDatabase]
    public IExtractionConfiguration[] ExtractionConfigurations
    {
        get
        {
            return
                Repository.GetAllObjectsWithParent<SelectedDataSets>(this)
                    .Select(sds => sds.ExtractionConfiguration)
                    .ToArray();
        }
    }

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ICatalogue Catalogue => _catalogue.Value;

    #endregion


    public ExtractableDataSet()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Defines that the given Catalogue is extractable to researchers as a data set, this is stored in the DataExport database
    /// </summary>
    /// <returns></returns>
    public ExtractableDataSet(IDataExportRepository repository, ICatalogue catalogue, bool disableExtraction = false)
    {
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "DisableExtraction", disableExtraction },
            { "Catalogue_ID", catalogue.ID }
        });

        ClearAllInjections();
        InjectKnown(catalogue);
    }

    internal ExtractableDataSet(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Catalogue_ID = Convert.ToInt32(r["Catalogue_ID"]);
        DisableExtraction = (bool)r["DisableExtraction"];

        ClearAllInjections();
    }

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public bool IsCatalogueDeprecated => Catalogue == null || Catalogue.IsDeprecated;

    /// <summary>
    /// Returns the <see cref="ICatalogue"/> behind this dataset's Name or a string describing the object state if the Catalogue is unreachable.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (Catalogue == null)
            return $"DELETED CATALOGUE {Catalogue_ID}";

        //only bother refreshing Catalogue details if we will be able to get a legit catalogue name
        return Catalogue.IsDeprecated ? $"DEPRECATED CATALOGUE {Catalogue.Name}" : Catalogue.Name;
    }

    #region Stuff for updating our internal database records

    /// <summary>
    /// Deletes the dataset, this will make the <see cref="ICatalogue"/> non extractable.  This operation fails if
    /// the dataset is part of any <see cref="ExtractionConfigurations"/>.
    /// </summary>
    public override void DeleteInDatabase()
    {
        try
        {
            Repository.DeleteFromDatabase(this);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("FK_SelectedDataSets_ExtractableDataSet"))
                throw new Exception(
                    $"Cannot delete {this} because it is in use by the following configurations :{Environment.NewLine}{string.Join(Environment.NewLine, ExtractionConfigurations.Select(c => $"{c.Name}({c.Project})"))}",
                    e);
            throw;
        }
    }

    #endregion

    /// <summary>
    /// Returns an object indicating whether the dataset is project specific or not
    /// </summary>
    /// <returns></returns>
    public CatalogueExtractabilityStatus GetCatalogueExtractabilityStatus() => new(true, Projects.Any());

    private Lazy<ICatalogue> _catalogue;

    /// <inheritdoc/>
    public void InjectKnown(ICatalogue instance)
    {
        if (instance.ID != Catalogue_ID)
            throw new ArgumentOutOfRangeException(nameof(instance),
                $"You told us our Catalogue was '{instance}' but its ID didn't match so that is NOT our Catalogue");
        _catalogue = new Lazy<ICatalogue>(instance);
    }

    /// <inheritdoc/>
    public void ClearAllInjections()
    {
        _catalogue = new Lazy<ICatalogue>(FetchCatalogue);
    }

    private ICatalogue FetchCatalogue()
    {
        try
        {
            var cata = ((IDataExportRepository)Repository).CatalogueRepository.GetObjectByID<Catalogue>(Catalogue_ID);
            cata.InjectKnown(GetCatalogueExtractabilityStatus());
            return cata;
        }
        catch (KeyNotFoundException)
        {
            //Catalogue has been deleted!
            return null;
        }
    }
}