// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Sharing;

/// <summary>
///     Opens a <see cref="ShareDefinition" /> (which must be a share of a <see cref="Catalogue" />) and imports all
///     descriptions including for CatalogueItems
/// </summary>
public class ExecuteCommandImportCatalogueDescriptionsFromShare : ExecuteCommandImportShare, IAtomicCommand
{
    private readonly Catalogue _targetCatalogue;

    public ExecuteCommandImportCatalogueDescriptionsFromShare(IBasicActivateItems activator,
        FileCollectionCombineable sourceFileCollection, Catalogue targetCatalogue) : base(activator,
        sourceFileCollection)
    {
        _targetCatalogue = targetCatalogue;
        UseTripleDotSuffix = true;
    }

    [UseWithObjectConstructor]
    public ExecuteCommandImportCatalogueDescriptionsFromShare(IBasicActivateItems activator, Catalogue targetCatalogue)
        : base(activator, null)
    {
        _targetCatalogue = targetCatalogue;
        UseTripleDotSuffix = true;
    }

    protected override void ExecuteImpl(ShareManager shareManager, List<ShareDefinition> shareDefinitions)
    {
        var first = shareDefinitions.First();

        if (first.Type != typeof(Catalogue))
            throw new Exception("ShareDefinition was not for a Catalogue");

        if (_targetCatalogue.Name != (string)first.Properties["Name"])
            if (!YesNo(
                    $"Catalogue Name is '{_targetCatalogue.Name}' but ShareDefinition is for, '{first.Properties["Name"]}'.  Import Anyway?",
                    "Import Anyway?"))
                return;

        ShareManager.ImportPropertiesOnly(_targetCatalogue, first);
        _targetCatalogue.SaveToDatabase();

        var liveCatalogueItems = _targetCatalogue.CatalogueItems;

        foreach (var sd in shareDefinitions.Skip(1))
        {
            if (sd.Type != typeof(CatalogueItem))
                throw new Exception(
                    $"Unexpected shared object of Type {sd.Type} (Expected ShareDefinitionList to have 1 Catalogue + N CatalogueItems)");

            var shareName = (string)sd.Properties["Name"];

            var existingMatch = liveCatalogueItems.FirstOrDefault(ci => ci.Name.Equals(shareName)) ??
                                new CatalogueItem(BasicActivator.RepositoryLocator.CatalogueRepository,
                                    _targetCatalogue, shareName);
            ShareManager.ImportPropertiesOnly(existingMatch, sd);
            existingMatch.SaveToDatabase();
        }

        Publish(_targetCatalogue);
    }
}