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
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeCatalogueProjectSpecific : BasicCommandExecution, IAtomicCommandWithTarget
{
    private ICatalogue _catalogue;
    private IProject _project;
    private List<int> _existingProjectIDs;
    private readonly bool _force = false;
    private readonly IBasicActivateItems _activator;

    [UseWithObjectConstructor]
    public ExecuteCommandMakeCatalogueProjectSpecific(IBasicActivateItems itemActivator, ICatalogue catalogue,
        IProject project, [DemandsInitialization("Ignore Validation",DemandType.Unspecified,defaultValue:false)]bool force) : this(itemActivator)
    {
        SetCatalogue(catalogue);
        _project = project;
        _force = force;
        _activator = itemActivator;
    }

    public ExecuteCommandMakeCatalogueProjectSpecific(IBasicActivateItems itemActivator) : base(itemActivator)
    {
        UseTripleDotSuffix = true;
        _activator = itemActivator;
    }

    public override string GetCommandHelp() =>
        "Restrict use of the dataset only to extractions of the specified Project";

    public override void Execute()
    {
        if (_catalogue == null)
            SetCatalogue(SelectOne(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().ToList()));
        if(_existingProjectIDs is null)
            GetExistingProjectIDs();

        _project ??= SelectOne<Project>(GetListOfValidProjects());

        if (_project == null || _catalogue == null)
            return;

        base.Execute();

        ProjectSpecificCatalogueManager.MakeCatalogueProjectSpecific(BasicActivator.RepositoryLocator.DataExportRepository, _catalogue, _project);
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


    private List<Project> GetListOfValidProjects()
    {
        var dataExportChildProvider = ((DataExportChildProvider)_activator.CoreChildProvider);

        var availableProjects = dataExportChildProvider.Projects.Where(p =>!dataExportChildProvider.ExtractableDataSetProjects.Where(edsp => edsp.Project_ID == p.ID).Select(edsp => edsp.DataSet.Catalogue).Contains(_catalogue));
        return availableProjects.Where(p => ProjectSpecificCatalogueManager.CanMakeCatalogueProjectSpecific(dataExportChildProvider, _catalogue, p, _existingProjectIDs)).ToList();
    }

    private void GetExistingProjectIDs()
    {  
        var dataExportChildProvider = ((DataExportChildProvider)_activator.CoreChildProvider);
        var existingProjects = dataExportChildProvider.Projects.Where(p => dataExportChildProvider.ExtractableDataSetProjects.Where(edsp => edsp.Project_ID==p.ID).Select(edsp => edsp.DataSet.Catalogue).Contains(_catalogue));
        _existingProjectIDs = existingProjects.Select(p => p.ID).ToList();
    }

    private void SetCatalogue(ICatalogue catalogue)
    {
        ResetImpossibleness();
        var dataExportChildProvider = ((DataExportChildProvider)_activator.CoreChildProvider);
        _catalogue = catalogue;
        GetExistingProjectIDs();
        if (catalogue == null)
        {
            SetImpossible("Catalogue cannot be null");
            return;
        }
        var eds = dataExportChildProvider.ExtractableDataSets.Where(eds => eds.Catalogue_ID == catalogue.ID).ToList();
        var status = eds.Count == 0 ? new CatalogueExtractabilityStatus(false, false) : new CatalogueExtractabilityStatus(true, eds.Count > 1 ? true : eds.First().Projects.Any());

        if (!GetListOfValidProjects().Any() && !_force)
        {
            SetImpossible("No valid Projects available");
        }

        if (!status.IsExtractable)
            SetImpossible("Catalogue must first be made Extractable");

        var ei = dataExportChildProvider.AllCatalogueItems.Where(ci => ci.Catalogue_ID == _catalogue.ID && ci.ExtractionInformation != null).Select(ci => ci.ExtractionInformation);
        if (!ei.Any())
            SetImpossible("Catalogue has no extractable columns");

        if (ei.Count(e => e.IsExtractionIdentifier) < 1)
            SetImpossible("Catalogue must have at least 1 IsExtractionIdentifier column");

    }
}