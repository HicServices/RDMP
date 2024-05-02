// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;

/// <summary>
///     Creates a new <see cref="Catalogue" /> reference in the RDMP database pointing to a table (which must already
///     exist) in a relational database
/// </summary>
public class ExecuteCommandCreateNewCatalogueByImportingExistingDataTable : CatalogueCreationCommandExecution
{
    private readonly DiscoveredTable _importTable;


    public ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(IBasicActivateItems activator) : this(activator,
        null, null, null)
    {
        UseTripleDotSuffix = true;
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(IBasicActivateItems activator,
        [DemandsInitialization("An existing table that you want to import a reference to")]
        DiscoveredTable existingTable,
        [DemandsInitialization(Desc_ProjectSpecificParameter)]
        IProject projectSpecific,
        [DemandsInitialization(Desc_TargetFolder, DefaultValue = "\\")]
        string targetFolder = "\\") : base(activator, projectSpecific, targetFolder)
    {
        _importTable = existingTable;
    }

    public override void Execute()
    {
        base.Execute();

        var tbl = _importTable ?? BasicActivator.SelectTable(false, "Table to import");

        if (tbl == null)
            return;

        var importer = new TableInfoImporter(BasicActivator.RepositoryLocator.CatalogueRepository, tbl);
        importer.DoImport(out var ti, out _);

        var c = BasicActivator.CreateAndConfigureCatalogue(ti, null, "Existing table", ProjectSpecific, TargetFolder);

        if (c?.Exists() == true) return;
        if (BasicActivator.IsInteractive
            && BasicActivator.YesNo(
                "You have cancelled Catalogue creation.  Do you want to delete the TableInfo metadata reference (this will not affect any database tables)?",
                "Delete TableInfo", out var chosen)
            && chosen)
            ti.DeleteInDatabase();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Import);
    }

    public override string GetCommandHelp()
    {
        return GlobalStrings.CreateNewCatalogueByImportingExistingDataTableHelp;
    }

    public override string GetCommandName()
    {
        return OverrideCommandName ?? GlobalStrings.CreateNewCatalogueByImportingExistingDataTable;
    }
}