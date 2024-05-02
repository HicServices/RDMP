// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     View/run the extraction SQL for a given <see cref="Catalogue" /> in a given <see cref="ExtractionConfiguration" />
/// </summary>
public class ExecuteCommandViewExtractionSql : ExecuteCommandViewDataBase, IAtomicCommandWithTarget
{
    private IExtractionConfiguration _extractionConfiguration;
    private ISelectedDataSets _selectedDataSet;

    [UseWithObjectConstructor]
    public ExecuteCommandViewExtractionSql(IBasicActivateItems activator,
        [DemandsInitialization("The extraction configuration you want to know about")]
        ExtractionConfiguration ec,
        [DemandsInitialization("The dataset for whom you want to see the extraction SQL")]
        Catalogue c,
        [DemandsInitialization(ToFileDescription)]
        FileInfo toFile = null) : base(activator, toFile)
    {
        _extractionConfiguration = ec;

        _selectedDataSet = ec.SelectedDataSets.FirstOrDefault(sds => sds.GetCatalogue().Equals(c));

        if (_selectedDataSet == null) SetImpossible($"Catalogue '{c}' is not listed as a selected dataset in '{ec}'");
    }

    public ExecuteCommandViewExtractionSql(IBasicActivateItems activator,
        ExtractionConfiguration extractionConfiguration)
        : base(activator, null)
    {
        _extractionConfiguration = extractionConfiguration;
    }

    public ExecuteCommandViewExtractionSql(IBasicActivateItems activator,
        SelectedDataSets sds)
        : base(activator, null)
    {
        _extractionConfiguration = sds.ExtractionConfiguration;
        _selectedDataSet = sds;
    }

    public ExecuteCommandViewExtractionSql(IBasicActivateItems activator) : base(activator, null)
    {
    }

    public override string GetCommandHelp()
    {
        return
            "Shows the SQL that will be executed for the given dataset when it is extracted including the linkage with the cohort table";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Execute);
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        if (target is SelectedDataSets sets)
        {
            _selectedDataSet = sets;

            if (_selectedDataSet != null)
                //must have datasets and have a cohort configured
                if (_selectedDataSet.ExtractionConfiguration.Cohort_ID == null)
                    SetImpossible("No cohort has been selected for ExtractionConfiguration");
        }

        if (target is ExtractionConfiguration configuration)
            _extractionConfiguration = configuration;

        return this;
    }

    protected override IViewSQLAndResultsCollection GetCollection()
    {
        var sds = _selectedDataSet;

        if (sds == null && _extractionConfiguration != null)
            sds = SelectOne(
                BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<SelectedDataSets>(
                    _extractionConfiguration));

        return _selectedDataSet == null
            ? null
            : (IViewSQLAndResultsCollection)new ViewSelectedDatasetExtractionUICollection(sds);
    }
}