// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewLoadMetadata : BasicCommandExecution, IAtomicCommandWithTarget
{
    private Catalogue[] _availableCatalogues;
    private Catalogue _catalogue;

    /// <summary>
    /// The folder to put the new <see cref="LoadMetadata"/> in.  Defaults to <see cref="FolderHelper.Root"/>
    /// </summary>
    public string Folder { get; set; } = FolderHelper.Root;

    public ExecuteCommandCreateNewLoadMetadata(IBasicActivateItems activator,
        [DemandsInitialization(
            "Which Catalogue does this load.")]
        Catalogue catalogue = null) : base(activator)
    {
        _availableCatalogues =
            activator.CoreChildProvider.AllCatalogues.Value.ToArray();

        if (!_availableCatalogues.Any())
            SetImpossible("There are no available Catalogues");

        if (catalogue != null) SetTarget(catalogue);

        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp() =>
        "Create a new data load configuration for loading data into a given set of datasets through RAW=>STAGING=>LIVE migration / adjustment";

    public override void Execute()
    {
        base.Execute();

        var catalogueBefore = _catalogue;
        try
        {
            //if we don't have an explicit one picked yet
            if (_catalogue == null)
                if (!SelectOne(_availableCatalogues, out _catalogue)) //get user to pick one
                    return; //user cancelled

            //create the load
            var lmd = new LoadMetadata(_catalogue.CatalogueRepository, $"Loading {_catalogue.Name}");

            lmd.EnsureLoggingWorksFor(_catalogue);

            _catalogue.SaveToDatabase();

            lmd.Folder = Folder;
            lmd.SaveToDatabase();

            lmd.LinkToCatalogue(_catalogue);
            Publish(lmd);

            Activate(lmd);
        }
        finally
        {
            _catalogue = catalogueBefore;
        }
    }


    public override string GetCommandName() => "Create New Data Load Configuration...";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.LoadMetadata, OverlayKind.Add);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _catalogue = (Catalogue)target;
        return this;
    }
}