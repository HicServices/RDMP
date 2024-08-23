// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCloneExtractionConfiguration : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _extractionConfiguration;
    private readonly IBasicActivateItems _activeItems;
    private readonly List<IExtractableDataSet> toRemove = [];
    private readonly List<Catalogue> toAdd = [];
    private void CheckForDeprecatedCatalogues()
    {
        if (_extractionConfiguration.SelectedDataSets.Any(sd => sd.GetCatalogue().IsDeprecated) && _activeItems.IsInteractive)
        {
            if (YesNo("There are Deprecated catalogues in this Extraction Configuration. Would you like to replace them with their replacement (where available)?", "Replace Deprecated Catalogues"))
            {
                var repo = _activeItems.RepositoryLocator.CatalogueRepository;
                var DeprecatedDatasets = _extractionConfiguration.SelectedDataSets.Where(sd => sd.GetCatalogue().IsDeprecated).ToList();
                var replacedBy = repo.GetExtendedProperties(ExtendedProperty.ReplacedBy);
                foreach (ISelectedDataSets ds in DeprecatedDatasets)
                {
                    var replacement = replacedBy.Where(rb => rb.ReferencedObjectID == ds.GetCatalogue().ID).FirstOrDefault();
                    if (replacement is not null)
                    {
                        var replacementCatalogue = repo.GetObjectByID<Catalogue>(Int32.Parse(replacement.Value));
                        while (replacementCatalogue.IsDeprecated)
                        {
                            var replacementCatalogueIsReplacedBy = replacedBy.Where(rb => rb.ReferencedObjectID == replacementCatalogue.ID).FirstOrDefault();
                            if(replacementCatalogueIsReplacedBy is not null)
                            {
                                //have found further down the tree
                                replacementCatalogue = repo.GetObjectByID<Catalogue>(Int32.Parse(replacementCatalogueIsReplacedBy.Value));
                            }
                            else
                            {
                                //there is no replacement
                                break;
                            }
                        }
                        toRemove.Add(ds.ExtractableDataSet);
                        toAdd.Add(replacementCatalogue);
                    }
                }


            }
        }
    }

    public ExecuteCommandCloneExtractionConfiguration(IBasicActivateItems activator,
        ExtractionConfiguration extractionConfiguration) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;
        _activeItems = activator;
        if (!_extractionConfiguration.SelectedDataSets.Any())
            SetImpossible("ExtractionConfiguration does not have any selected datasets");
    }

    public override string GetCommandHelp() =>
        "Creates an exact copy of the Extraction Configuration including the cohort selection, all selected datasets, parameters, filter containers, filters etc";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.CloneExtractionConfiguration);

    public override void Execute()
    {
        base.Execute();
        CheckForDeprecatedCatalogues();

        var clone = _extractionConfiguration.DeepCloneWithNewIDs();
        foreach (ExtractableDataSet ds in toRemove)
        {
            clone.RemoveDatasetFromConfiguration(ds);
        }
        foreach (Catalogue c in toAdd)
        {
            //check if the eds already exis
            var eds = _activeItems.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", c.ID).FirstOrDefault();
            if (eds is null)
            {
                eds = new ExtractableDataSet(_activeItems.RepositoryLocator.DataExportRepository, c);
                eds.SaveToDatabase();
            }
            clone.AddDatasetToConfiguration(eds);
        }
        Publish((DatabaseEntity)clone.Project);
        Emphasise(clone);
    }
}