// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeProjectSpecificCatalogueNormalAgain : BasicCommandExecution, IAtomicCommand
{
    private Catalogue _catalogue;
    private List<ExtractableDataSet> _extractableDataSets;
    private ExtractableDataSet _extractableDataSet;
    private Project _selectedProj;

    public ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(IBasicActivateItems activator, Catalogue catalogue, ExtractableDataSet eds) :
        base(activator)
    {
        _catalogue = catalogue;
        _extractableDataSet = eds;


        var dataExportRepository = BasicActivator.RepositoryLocator.DataExportRepository;
        if (dataExportRepository == null)
        {
            SetImpossible("Data Export functionality is not available");
            return;
        }
    }

    public override string GetCommandHelp() =>
        "Take a dataset that was previously only usable with extractions of a specific project and make it free for use in any extraction project";

    public override void Execute()
    {
        var dataExportRepository = BasicActivator.RepositoryLocator.DataExportRepository;

        base.Execute();
        _extractableDataSets = dataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(_catalogue).Where(eds => eds.Projects.Any() && eds.Projects.Select(p => ProjectSpecificCatalogueManager.CanMakeCatalogueNonProjectSpecific(dataExportRepository, _catalogue, eds, p)).Contains(true)).ToList();
        if (!_extractableDataSets.Any())
        {
            SetImpossible("Cannot make Catalogue Non-Project specific");
            return;
        }



        if (_extractableDataSet is null)
        {

            var projectIds = _extractableDataSets.SelectMany(eds => eds.Projects.Where(p => ProjectSpecificCatalogueManager.CanMakeCatalogueNonProjectSpecific(dataExportRepository,_catalogue, eds,p))).Select(p => p.ID);
            _selectedProj = SelectOne<Project>(BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjectsInIDList<Project>(projectIds).ToList());
            if (_selectedProj is null) return;
            _extractableDataSet = _extractableDataSets.FirstOrDefault(eds => eds.Projects.Select(p => p.ID).Contains(_selectedProj.ID));
        }
        if (_extractableDataSet == null)
        {
            return;
        }

        if (!_extractableDataSet.Projects.Any())
        {
            SetImpossible("Catalogue is not a project specific Catalogue");
            return;
        }

        ProjectSpecificCatalogueManager.MakeCatalogueNonProjectSpecific(BasicActivator.RepositoryLocator.DataExportRepository, _catalogue, _extractableDataSet,_selectedProj);

        Publish(_catalogue);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.MakeProjectSpecificCatalogueNormalAgain);
}