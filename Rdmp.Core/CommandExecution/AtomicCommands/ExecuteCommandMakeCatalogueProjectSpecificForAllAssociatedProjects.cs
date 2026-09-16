// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeCatalogueProjectSpecificForAllAssociatedProjects : BasicCommandExecution, IAtomicCommandWithTarget
{
    private ICatalogue _catalogue;

    private readonly bool _force = false;
    private bool _hasRanCatalogueValidation = false;
    private readonly IBasicActivateItems _activator;

    [UseWithObjectConstructor]
    public ExecuteCommandMakeCatalogueProjectSpecificForAllAssociatedProjects(IBasicActivateItems itemActivator, ICatalogue catalogue,
        [DemandsInitialization("Ignore Validation", DemandType.Unspecified, defaultValue: false)] bool force) : this(itemActivator)
    {
        _catalogue = catalogue;
        _force = force;
        _activator = itemActivator;
    }

    public ExecuteCommandMakeCatalogueProjectSpecificForAllAssociatedProjects(IBasicActivateItems itemActivator) : base(itemActivator)
    {
        UseTripleDotSuffix = true;
        _activator = itemActivator;
    }

    public override string GetCommandHelp() =>
        "Restrict use of the dataset only to extractions of associated Projects";

    public override void Execute()
    {
        if (_catalogue == null)
        {
            var catalogues = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().ToList();
            if (!catalogues.Any())
            {
                Show($"No valid catalogues found to make project specific.");
                return;
            }
            SetCatalogue(SelectOne(catalogues));
        }
        if (!_hasRanCatalogueValidation)
        {
            SetCatalogue(_catalogue);
        }

        var dataExportChildProvider = ((DataExportChildProvider)_activator.CoreChildProvider);
        var eds = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(_catalogue);
        var projects = eds.SelectMany(e => e.ExtractionConfigurations.Select(ec => ec.Project));
        if (YesNo($"""
            You are about to make Catalogue {_catalogue.Name} project specific.
            It will be associated with the following projects:
            {string.Join(Environment.NewLine, projects.Select(p => $"{p.ProjectNumber}: {p.Name}"))}

            Are you sure you want to continue?
            ""","Make Catalogue Project Specific"))
        {
            foreach (var project in projects)
            {
                var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator, _catalogue, project, _force);
                cmd.Execute();
            }
        }

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
        //var status = _catalogue.GetExtractabilityStatus(BasicActivator.RepositoryLocator.DataExportRepository);
        //if (!GetListOfValidProjects(out string worstReason).Any() && !_force)
        //{
        //    SetImpossible($"No valid Projects available.Reason: {worstReason}");
        //}

        //if (!status.IsExtractable)
        //    SetImpossible("Catalogue must first be made Extractable");


        //var ei = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
        //if (!ei.Any())
        //    SetImpossible("Catalogue has no extractable columns");

        //if (ei.Count(e => e.IsExtractionIdentifier) < 1)
        //    SetImpossible("Catalogue must have at least 1 IsExtractionIdentifier column");

        _hasRanCatalogueValidation = true;
    }
}