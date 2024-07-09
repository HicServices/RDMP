// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace Rdmp.Core.CommandExecution.AtomicCommands.Sharing;

public class ExecuteCommandImportShareDefinitionList : BasicCommandExecution, IAtomicCommand
{
    public ExecuteCommandImportShareDefinitionList(IBasicActivateItems activator) : base(activator)
    {
    }

    public override void Execute()
    {
        base.Execute();

        var selected =
            BasicActivator.SelectFiles("Select ShareDefinitions to import", "Sharing Definition File", "*.sd");

        if (selected != null && selected.Any())
            try
            {
                var shareManager = new ShareManager(BasicActivator.RepositoryLocator)
                {
                    LocalReferenceGetter = LocalReferenceGetter
                };

                foreach (var f in selected)
                {
                    using var stream = File.Open(f.FullName, FileMode.Open);
                    shareManager.ImportSharedObject(stream);
                }
            }
            catch (Exception e)
            {
                BasicActivator.ShowException("Error importing file(s)", e);
            }
    }


    public override string GetCommandHelp() =>
        "Import serialized RDMP objects that have been shared with you in a share definition file.  If you already have the objects then they will be updated to match the file.";

    private int? LocalReferenceGetter(PropertyInfo property, RelationshipAttribute relationshipAttribute,
        ShareDefinition shareDefinition)
    {
        BasicActivator.Show(
            $"Choose a local object for '{property}' on {Environment.NewLine}{string.Join(Environment.NewLine, shareDefinition.Properties.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

        var requiredType = relationshipAttribute.Cref;

        if (BasicActivator.RepositoryLocator.CatalogueRepository.SupportsObjectType(requiredType))
        {
            var selected = SelectOne(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects(requiredType)
                .Cast<DatabaseEntity>().ToArray());
            if (selected != null)
                return selected.ID;
        }

        if (BasicActivator.RepositoryLocator.DataExportRepository.SupportsObjectType(requiredType))
        {
            var selected = SelectOne(BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects(requiredType)
                .Cast<DatabaseEntity>().ToArray());
            if (selected != null)
                return selected.ID;
        }

        return null;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(FamFamFamIcons.page_white_get);
}