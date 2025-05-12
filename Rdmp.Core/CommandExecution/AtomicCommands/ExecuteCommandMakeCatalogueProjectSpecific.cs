// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeCatalogueProjectSpecific : BasicCommandExecution, IAtomicCommandWithTarget
{
    private ICatalogue _catalogue;
    private IProject _project;

    [UseWithObjectConstructor]
    public ExecuteCommandMakeCatalogueProjectSpecific(IBasicActivateItems itemActivator, ICatalogue catalogue,
        IProject project) : this(itemActivator)
    {
        SetCatalogue(catalogue);
        _project = project;
    }


    public List<int> projectIdsToIgnore = [];

    public ExecuteCommandMakeCatalogueProjectSpecific(IBasicActivateItems itemActivator) : base(itemActivator)
    {
        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp() =>
        "Restrict use of the dataset only to extractions of the specified Project";

    public override void Execute()
    {
        if (_catalogue == null)
            SetCatalogue(SelectOne<Catalogue>(BasicActivator.RepositoryLocator.CatalogueRepository));

        _project ??= SelectOne<Project>(BasicActivator.RepositoryLocator.DataExportRepository);

        if (_project == null || _catalogue == null)
            return;

        base.Execute();

        var eds = BasicActivator.RepositoryLocator.DataExportRepository
            .GetAllObjectsWithParent<ExtractableDataSet>(_catalogue).Where(e => e.Project_ID is null).SingleOrDefault();

        if (eds is not null)
        {
            var alreadyInConfiguration = eds.ExtractionConfigurations.FirstOrDefault(ec => ec.Project_ID != _project.ID && !projectIdsToIgnore.Contains(ec.Project_ID));

            if (alreadyInConfiguration != null)
                throw new Exception(
                    $"Cannot make {_catalogue} Project Specific because it is already a part of ExtractionConfiguration {alreadyInConfiguration} (Project={alreadyInConfiguration.Project}) and possibly others");

            eds.Project_ID = _project.ID;
            foreach (var ei in _catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Where(ei => ei.ExtractionCategory is ExtractionCategory.Core))
            {
                ei.ExtractionCategory = ExtractionCategory.ProjectSpecific;
                ei.SaveToDatabase();
            }

            eds.SaveToDatabase();
        }
        else
        {
            eds = new ExtractableDataSet(BasicActivator.RepositoryLocator.DataExportRepository, _catalogue);
            eds.Project_ID = _project.ID;
            foreach (var ei in _catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Where(ei => ei.ExtractionCategory is ExtractionCategory.Core))
            {
                ei.ExtractionCategory = ExtractionCategory.ProjectSpecific;
                ei.SaveToDatabase();
            }
            eds.SaveToDatabase();
        }

        Publish(_catalogue);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.ProjectCatalogue);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        switch (target)
        {
            case Catalogue catalogue:
                SetCatalogue(catalogue);
                break;
            case Project project:
                _project = project;
                break;
        }

        return this;
    }

    private void SetCatalogue(ICatalogue catalogue)
    {
        ResetImpossibleness();

        _catalogue = catalogue;

        if (catalogue == null)
        {
            SetImpossible("Catalogue cannot be null");
            return;
        }

        var status = _catalogue.GetExtractabilityStatus(BasicActivator.RepositoryLocator.DataExportRepository);

        var eds = BasicActivator.RepositoryLocator.DataExportRepository
         .GetAllObjectsWithParent<ExtractableDataSet>(_catalogue).Where(e => e.Project_ID is null).SingleOrDefault();

        if (eds is not null)
        {
            var alreadyInConfiguration = eds.ExtractionConfigurations.Where(ec => ec.Project_ID != _project.ID && !projectIdsToIgnore.Contains(ec.Project_ID));

            if (alreadyInConfiguration != null)
                foreach (var aic in alreadyInConfiguration)
                {
                    SetImpossible(
                         $"Cannot make {_catalogue} Project Specific because it is already a part of ExtractionConfiguration {aic} (Project={aic.Project}) and possibly others");
                }

        }
        if (!status.IsExtractable)
            SetImpossible("Catalogue must first be made Extractable");

        var ei = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
        if (!ei.Any())
            SetImpossible("Catalogue has no extractable columns");

        if (ei.Count(e => e.IsExtractionIdentifier) < 1)
            SetImpossible("Catalogue must have at least 1 IsExtractionIdentifier column");

    }
}