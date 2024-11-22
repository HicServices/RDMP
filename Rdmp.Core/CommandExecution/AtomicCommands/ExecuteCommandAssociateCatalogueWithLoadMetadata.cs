// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAssociateCatalogueWithLoadMetadata : BasicCommandExecution
{
    private readonly LoadMetadata _loadMetadata;
    private readonly Catalogue[] _availableCatalogues;
    private readonly ICatalogue[] _otherCatalogues;
    private Catalogue[] _chosenCatalogues;

    [UseWithObjectConstructor]
    public ExecuteCommandAssociateCatalogueWithLoadMetadata(IBasicActivateItems activator, LoadMetadata loadMetadata,
        Catalogue[] toAssociate) : this(activator, loadMetadata)
    {
        //if command is possible, select those that are available for association
        if (!IsImpossible)
        {
            _chosenCatalogues = _availableCatalogues.Intersect(toAssociate).ToArray();

            if (_chosenCatalogues.Length == 0)
                SetImpossible(
                    $"None of the provided Catalogues are available for association with the LoadMetadata '{loadMetadata}'");
        }
    }

    public ExecuteCommandAssociateCatalogueWithLoadMetadata(IBasicActivateItems activator, LoadMetadata loadMetadata) :
        base(activator)
    {
        _loadMetadata = loadMetadata;

        var cataloguesAlreadyUsedByLoadMetadata = _loadMetadata.CatalogueRepository.GetAllObjectsWhere<LoadMetadataCatalogueLinkage>("LoadMetadataID", loadMetadata.ID).Select(l => l.CatalogueID);
        _availableCatalogues = BasicActivator.CoreChildProvider.AllCatalogues.Where(c => !cataloguesAlreadyUsedByLoadMetadata.Contains(c.ID)).ToArray();
        //Ensure logging task is correct
        _otherCatalogues = _loadMetadata.GetAllCatalogues().ToArray();

        if (!_availableCatalogues.Any())
            SetImpossible("There are no Catalogues that are not associated with another Load already");
    }

    public override string GetCommandHelp() =>
        "Specifies that the table(s) underlying the dataset are loaded by the load configuration.  The union of all catalogue(s) table(s) will be used for RAW=>STAGING=>LIVE migration during DLE execution";

    public override void Execute()
    {
        base.Execute();

        if (_chosenCatalogues == null && !SelectMany(_availableCatalogues, out _chosenCatalogues))
            return;

        foreach (var cata in _chosenCatalogues)
        {
            //if there are other catalogues
            if (_otherCatalogues.Any())
            {
                var tasks = _otherCatalogues.Select(c => c.LoggingDataTask).Distinct().ToArray();
                //if the other catalogues have an agreed logging task
                if (tasks.Length == 1)
                {
                    var task = tasks.Single();

                    //and that logging task is not blank!, and differs from this Catalogue
                    if (!string.IsNullOrWhiteSpace(task) && !task.Equals(cata.LoggingDataTask))
                    {
                        var liveServers = _otherCatalogues.Where(c => c.LiveLoggingServer_ID != null)
                            .Select(c => c.LiveLoggingServer_ID).Distinct().ToArray();

                        //AND if there is agreement on what logging server to use!
                        if (liveServers.Length <= 1)
                            //if there is no current logging task for the Catalogue
                            if (string.IsNullOrWhiteSpace(cata.LoggingDataTask)
                                //or if the user wants to switch to the new one
                                || YesNo(
                                    $"Do you want to set Catalogue '{cata.Name}' to use shared logging task '{task}' instead of its current Logging Task '{cata.LoggingDataTask}' (All Catalogues in a load must share the same task and logging servers)?",
                                    "Synchronise Logging Tasks"))
                            {
                                //switch Catalogue to use that logging task (including servers)
                                cata.LoggingDataTask = task;
                                cata.LiveLoggingServer_ID = liveServers.SingleOrDefault();
                            }
                    }
                }
                else if (tasks.Length == 0)
                {
                    cata.LoggingDataTask = $"Loading {_loadMetadata.Name}";
                }
            }
            cata.SaveToDatabase();
            //associate them
            _loadMetadata.LinkToCatalogue(cata);
        }

        Publish(_loadMetadata);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);

    public ICommandExecution SetTarget(Catalogue[] catalogues)
    {
        _chosenCatalogues = catalogues;
        if (_otherCatalogues.Any(catalogues.Contains))
            SetImpossible("Catalogue(s) are already part of the LoadMetadata");

        return this;
    }
}