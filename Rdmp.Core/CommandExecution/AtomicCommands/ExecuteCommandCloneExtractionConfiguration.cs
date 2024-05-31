// Copyright (c) The University of Dundee 2018-2019
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
    private IBasicActivateItems _activeItems;
    List<IExtractableDataSet> toRemove = new();
    List<Catalogue> toAdd = new();
    private void CheckForDepricatedCatalogues()
    {
        if (_extractionConfiguration.SelectedDataSets.Any(sd => sd.GetCatalogue().IsDeprecated) && _activeItems.IsInteractive)
        {
            if (YesNo("Replace Depricated Catalogues", "There are depricated catalogues in this Extraction Configuration. Would you like to replace them with their replacement (where available)?"))
            {
                var depricatedDatasets = _extractionConfiguration.SelectedDataSets.Where(sd => sd.GetCatalogue().IsDeprecated).ToList();
                var replacedBy = _activeItems.RepositoryLocator.CatalogueRepository.GetExtendedProperties(ExtendedProperty.ReplacedBy);
                foreach (ISelectedDataSets ds in depricatedDatasets)
                {
                    var replacement = replacedBy.Where(rb => rb.ReferencedObjectID == ds.GetCatalogue().ID).FirstOrDefault();
                    if (replacement is not null)
                    {
                        toRemove.Add(ds.ExtractableDataSet);
                        toAdd.Add(_activeItems.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(Int32.Parse(replacement.Value)));
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
        CheckForDepricatedCatalogues();

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
        Console.WriteLine('a');
        //cataRepo.GetExtendedProperties(ExtendedProperty.ReplacedBy, Deprecated))
        Publish((DatabaseEntity)clone.Project);
        Emphasise(clone);
    }
}