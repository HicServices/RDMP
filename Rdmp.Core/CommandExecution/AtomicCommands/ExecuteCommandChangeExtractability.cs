// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandChangeExtractability : BasicCommandExecution
{
    private readonly Catalogue _catalogue;
    private readonly bool _markExtractable;

    public ExecuteCommandChangeExtractability(IBasicActivateItems activator, Catalogue catalogue,
        bool? explicitExtractability = null) : base(activator)
    {
        _catalogue = catalogue;
        var status = _catalogue.GetExtractabilityStatus(BasicActivator.RepositoryLocator.DataExportRepository); 
        if (status == null)
        {
            SetImpossible(
                "We don't know whether Catalogue is extractable or not (possibly no Data Export database is available)");
            return;
        }

        if (status.IsProjectSpecific)
        {
            SetImpossible(
                "Cannot change the extractability because it is configured as a 'Project Specific Catalogue'");
            return;
        }

        // mark it extractable true/false as passed in constructor or just flip its state
        _markExtractable = explicitExtractability ?? !status.IsExtractable;
    }

    public override string GetCommandName() => _markExtractable ? "Mark Extractable" : "Mark Not Extractable";

    public override string GetCommandHelp() =>
        !_markExtractable
            ? "Prevent dataset from being released in Project extracts.  This fails if it is already part of any ExtractionConfigurations"
            : @"Enable dataset linkage\extraction in Project extracts.  This requires that at least one column be marked IsExtractionIdentifier";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractableDataSet, _markExtractable ? OverlayKind.Add : OverlayKind.Delete);

    public override void Execute()
    {
        base.Execute();

        if (_markExtractable)
        {
            if (_catalogue.GetExtractabilityStatus(BasicActivator.RepositoryLocator.DataExportRepository).IsExtractable)
            {
                Show($"{_catalogue} is already extractable");
            }
            else
            {
                new ExtractableDataSet(BasicActivator.RepositoryLocator.DataExportRepository, _catalogue);
                Publish(_catalogue);
            }
        }
        else
        {
            var extractabilityRecord =
                ((DataExportChildProvider)BasicActivator.CoreChildProvider).ExtractableDataSets.SingleOrDefault(ds =>
                    ds.Catalogue_ID == _catalogue.ID);
            if (extractabilityRecord == null)
            {
                Show($"{_catalogue} is already non-extractable");
            }
            else
            {
                extractabilityRecord.DeleteInDatabase();
                Publish(_catalogue);
            }
        }
    }
}