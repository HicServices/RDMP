// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandImportTableInfo : BasicCommandExecution
{
    private readonly DiscoveredTable _table;
    private readonly bool _createCatalogue;

    public ExecuteCommandImportTableInfo(IBasicActivateItems activator,
        [DemandsInitialization("The table or view you want to reference from RDMP.  See PickTable for syntax")]
        DiscoveredTable table,
        [DemandsInitialization("True to create a Catalogue as well as a TableInfo")]
        bool createCatalogue) : base(activator)
    {
        _table = table;
        _createCatalogue = createCatalogue;
    }

    public override void Execute()
    {
        base.Execute();

        ICatalogue c = null;
        ITableInfoImporter importer;

        var t = _table ?? SelectTable(false, "Select table to import");

        switch (t)
        {
            case null:
                return;
            //if it isn't a table valued function
            case DiscoveredTableValuedFunction function:
                importer = new TableValuedFunctionImporter(BasicActivator.RepositoryLocator.CatalogueRepository,
                    function);
                break;
            default:
                importer = new TableInfoImporter(BasicActivator.RepositoryLocator.CatalogueRepository, t);
                break;
        }

        importer.DoImport(out var ti, out var cis);

        BasicActivator.Show($"Successfully imported new TableInfo {ti.Name} with ID {ti.ID}");

        if (_createCatalogue)
        {
            var forwardEngineer = new ForwardEngineerCatalogue(ti, cis);
            forwardEngineer.ExecuteForwardEngineering(out c, out _, out _);

            BasicActivator.Show($"Successfully imported new Catalogue {c.Name} with ID {c.ID}");
        }

        Publish((IMapsDirectlyToDatabaseTable)c ?? ti);
    }

    public override string GetCommandName()
    {
        return "Import existing table (as new TableInfo)";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Add);
    }
}