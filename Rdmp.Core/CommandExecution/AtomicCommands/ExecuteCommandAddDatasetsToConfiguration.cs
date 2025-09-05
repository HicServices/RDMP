// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddDatasetsToConfiguration : BasicCommandExecution
{
    private readonly ExtractionConfiguration _targetExtractionConfiguration;

    private IExtractableDataSet[] _toadd;

    /// <summary>
    /// True if <see cref="_toadd"/> is a suggestion of available datasets from which the user must pick.
    /// False if <see cref="_toadd"/> reflects an already made selection (e.g. a drag and drop operation).
    /// </summary>
    private bool _userMustPick;

    public ExecuteCommandAddDatasetsToConfiguration(IBasicActivateItems activator,
        ExtractableDataSetCombineable sourceExtractableDataSetCombineable,
        ExtractionConfiguration targetExtractionConfiguration)
        : this(activator, targetExtractionConfiguration)
    {
        SetExtractableDataSets(false, sourceExtractableDataSetCombineable.ExtractableDataSets);
    }

    [UseWithObjectConstructor]
    public ExecuteCommandAddDatasetsToConfiguration(IBasicActivateItems itemActivator,
        ExtractableDataSet extractableDataSet, ExtractionConfiguration targetExtractionConfiguration)
        : this(itemActivator, targetExtractionConfiguration)
    {
        SetExtractableDataSets(false, extractableDataSet);
    }

    public ExecuteCommandAddDatasetsToConfiguration(IBasicActivateItems itemActivator,
        ExtractionConfiguration targetExtractionConfiguration) : base(itemActivator)
    {
        _targetExtractionConfiguration = targetExtractionConfiguration;

        if (_targetExtractionConfiguration.IsReleased)
            SetImpossible("Extraction is Frozen because it has been released and is readonly, try cloning it instead");

        //if we don't yet know what datasets to add (i.e. haven't called SetExtractableDataSets)
        if (_toadd == null)
            if (itemActivator.CoreChildProvider is DataExportChildProvider childProvider)
            {
                //use the ones that are not already in the ExtractionConfiguration
                var _datasets = childProvider.GetDatasets(targetExtractionConfiguration)
                    .Select(n => n.ExtractableDataSet).ToArray();
                var _importableDataSets = childProvider.ExtractableDataSets.Except(_datasets)

                    //where it can be used in any Project OR this project only
                    .Where(ds => !ds.Projects.Any()|| ds.Projects.Select(p => p.ID).Contains(targetExtractionConfiguration.Project_ID))
                    .ToArray();

                SetExtractableDataSets(true, _importableDataSets);
            }
            else
            {
                SetImpossible("CoreChildProvider was not DataExportChildProvider");
            }
    }

    private void SetExtractableDataSets(bool userMustPick, params IExtractableDataSet[] toAdd)
    {
        _userMustPick = userMustPick;
        var alreadyInConfiguration = _targetExtractionConfiguration.GetAllExtractableDataSets().ToArray();
        _toadd = toAdd.Except(alreadyInConfiguration).ToArray();

        if (!_toadd.Any())
            SetImpossible("ExtractionConfiguration already contains this dataset(s)");
    }

    public override void Execute()
    {
        base.Execute();

        if (_userMustPick)
        {
            if (!SelectMany(new DialogArgs
            {
                WindowTitle = "Select Datasets",
                TaskDescription =
                        "Select the Datasets you would like to be exported as part of your Extraction Configuration."
            }, _toadd.Cast<ExtractableDataSet>().ToArray(), out var selected))
                return;

            foreach (var ds in selected)
                _targetExtractionConfiguration.AddDatasetToConfiguration(ds);
        }
        else
        {
            foreach (var ds in _toadd)
                _targetExtractionConfiguration.AddDatasetToConfiguration(ds);
        }

        Publish(_targetExtractionConfiguration);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Import);
}