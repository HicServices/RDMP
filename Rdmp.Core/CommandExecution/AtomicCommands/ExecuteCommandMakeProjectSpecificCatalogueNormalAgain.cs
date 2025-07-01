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
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeProjectSpecificCatalogueNormalAgain : BasicCommandExecution, IAtomicCommand
{
    private Catalogue _catalogue;
    private List<ExtractableDataSet> _extractableDataSets;
    private ExtractableDataSet _extractableDataSet;

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

        _extractableDataSets = dataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(catalogue).Where(eds => eds.Project_ID != null && ProjectSpecificCatalogueManager.CanMakeCatalogueNonProjectSpecific(dataExportRepository, catalogue, eds)).ToList();
        if (!_extractableDataSets.Any())
        {
            SetImpossible("Cannot make Catalogue Non-Project specific");
            return;
        }
        

    }

    public override string GetCommandHelp() =>
        "Take a dataset that was previously only usable with extractions of a specific project and make it free for use in any extraction project";

    public override void Execute()
    {
        base.Execute();
        if (_extractableDataSet is null)
        {
            var projectIds = _extractableDataSets.Where(eds => eds.Project_ID != null).Select(eds => (int)eds.Project_ID);
            var selectedProj = SelectOne<Project>(BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjectsInIDList<Project>(projectIds).ToList());
            if (selectedProj is null) return;
            _extractableDataSet = _extractableDataSets.FirstOrDefault(eds => eds.Project_ID == selectedProj.ID);
        }
        if (_extractableDataSet == null)
        {
            return;
        }

        if (_extractableDataSet.Project_ID == null)
        {
            SetImpossible("Catalogue is not a project specific Catalogue");
            return;
        }

        ProjectSpecificCatalogueManager.MakeCatalogueNonProjectSpecific(BasicActivator.RepositoryLocator.DataExportRepository, _catalogue, _extractableDataSet);

        Publish(_catalogue);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.MakeProjectSpecificCatalogueNormalAgain);
}