// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="IProject"/>
public class Project : DatabaseEntity, IProject, ICustomSearchString, ICheckable, IHasFolder
{
    #region Database Properties

    private string _name;
    private string _masterTicket;
    private string _extractionDirectory;
    private int? _projectNumber;
    private string _folder;

    /// <inheritdoc/>
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    public string MasterTicket
    {
        get => _masterTicket;
        set => SetField(ref _masterTicket, value);
    }

    /// <inheritdoc/>
    [AdjustableLocation]
    public string ExtractionDirectory
    {
        get => _extractionDirectory;
        set => SetField(ref _extractionDirectory, value);
    }

    /// <inheritdoc/>
    [UsefulProperty]
    public int? ProjectNumber
    {
        get => _projectNumber;
        set => SetField(ref _projectNumber, value);
    }

    /// <inheritdoc/>
    [UsefulProperty]
    public string Folder
    {
        get => _folder;
        set => SetField(ref _folder, FolderHelper.Adjust(value));
    }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IExtractionConfiguration[] ExtractionConfigurations =>
        Repository.GetAllObjectsWithParent<ExtractionConfiguration>(this)
            .Cast<IExtractionConfiguration>()
            .ToArray();


    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IProjectCohortIdentificationConfigurationAssociation[]
        ProjectCohortIdentificationConfigurationAssociations =>
        Repository.GetAllObjectsWithParent<ProjectCohortIdentificationConfigurationAssociation>(this);

    #endregion

    public Project()
    {
    }

    /// <summary>
    /// Defines a new extraction project this is stored in the Data Export database
    /// </summary>
    public Project(IDataExportRepository repository, string name)
    {
        Repository = repository;

        try
        {
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                { "Name", name },
                { "Folder", FolderHelper.Root }
            });
        }
        catch (Exception ex)
        {
            //sometimes the user tries to create multiple Projects without fully populating the last one (with a project number)
            if (ex.Message.Contains("idx_ProjectNumberMustBeUnique"))
            {
                Project offender;
                try
                {
                    //find the one with the unset project number
                    offender = Repository.GetAllObjects<Project>().Single(p => p.ProjectNumber == null);
                }
                catch (Exception)
                {
                    throw;
                }

                throw new Exception(
                    $"Could not create a new Project because there is already another Project in the system ({offender}) which is missing a Project Number.  All projects must have a ProjectNumber, there can be 1 Project at a time which does not have a number and that is one that is being built by the user right now.  Either delete Project {offender} or give it a project number",
                    ex);
            }

            throw;
        }
    }

    internal Project(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        MasterTicket = r["MasterTicket"].ToString();
        Name = r["Name"] as string;
        ExtractionDirectory = r["ExtractionDirectory"] as string;

        ProjectNumber = ObjectToNullableInt(r["ProjectNumber"]);

        Folder = r["Folder"] as string ?? FolderHelper.Root;
    }

    /// <summary>
    /// Returns <see cref="Name"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;

    public void Check(ICheckNotifier notifier)
    {
        new ProjectChecker(new ThrowImmediatelyActivator(new RepositoryProvider(DataExportRepository), notifier), this)
            .Check(notifier);
    }

    /// <summary>
    /// Returns <see cref="ProjectNumber"/> (if any), <see cref="Name"/> and <see cref="MasterTicket"/>
    /// </summary>
    /// <returns></returns>
    public string GetSearchString() => ProjectNumber == null ? Name : $"{ProjectNumber}_{Name}_{MasterTicket}";

    /// <summary>
    /// Returns all <see cref="CohortIdentificationConfiguration"/> which are associated with the <see cref="IProject"/> (usually because
    /// they have been used to create <see cref="ExtractableCohort"/> used by the <see cref="IProject"/>).
    /// </summary>
    /// <returns></returns>
    public CohortIdentificationConfiguration[] GetAssociatedCohortIdentificationConfigurations()
    {
        var associations =
            Repository.GetAllObjectsWithParent<ProjectCohortIdentificationConfigurationAssociation>(this);
        return associations.Select(a => a.CohortIdentificationConfiguration).Where(c => c != null).ToArray();
    }

    /// <summary>
    /// Associates the <paramref name="cic"/> with the <see cref="IProject"/>.  This is usually done after generating an <see cref="IExtractableCohort"/>.
    /// You can associate a <see cref="CohortIdentificationConfiguration"/> with multiple <see cref="IProject"/> (M to M relationship).
    /// </summary>
    /// <param name="cic"></param>
    /// <returns></returns>
    public ProjectCohortIdentificationConfigurationAssociation
        AssociateWithCohortIdentification(CohortIdentificationConfiguration cic) =>
        new((IDataExportRepository)Repository, this, cic);

    /// <inheritdoc/>
    public ICatalogue[] GetAllProjectCatalogues()
    {
        return Repository.GetAllObjects<ExtractableDataSetProject>().Where(edsp => edsp.Project_ID == this.ID).Select(edsp => edsp.DataSet.Catalogue).ToArray(); 
    }

    /// <inheritdoc/>
    public ExtractionInformation[] GetAllProjectCatalogueColumns(ExtractionCategory c)
    {
        return GetAllProjectCatalogues().SelectMany(pc => pc.GetAllExtractionInformation(c)).ToArray();
    }

    /// <inheritdoc/>
    public ExtractionInformation[] GetAllProjectCatalogueColumns(ICoreChildProvider childProvider, ExtractionCategory c)
    {
        return childProvider is DataExportChildProvider dx
            ? dx.ExtractableDataSets.Where(eds => eds.Projects.Select(p=>p.ID).Contains(ID))
                .Select(e => dx.AllCataloguesDictionary[e.Catalogue_ID])
                .SelectMany(cata => cata.GetAllExtractionInformation(c)).ToArray()
            : GetAllProjectCatalogueColumns(c);
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn() => Array.Empty<IHasDependencies>();

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => ExtractionConfigurations;
}