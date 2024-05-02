// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Checks and updates TableInfo RDMP objects to reflect the latest state in the referenced database e.g. adding new
///     columns, removing old ones, updating datatype changes etc.
/// </summary>
public class ExecuteCommandSyncTableInfo : BasicCommandExecution
{
    private readonly ITableInfo _tableInfo;
    private readonly bool _alsoSyncAno;
    private readonly bool _autoYes;

    [UseWithObjectConstructor]
    public ExecuteCommandSyncTableInfo(IBasicActivateItems activator,
        [DemandsInitialization("The RDMP metadata object to synchronize with the underlying database state")]
        ITableInfo table,
        [DemandsInitialization(
            "True to also synchronize any ANOTables (anonymisation tables) associated with the TableInfo")]
        bool alsoSyncAno,
        [DemandsInitialization("True to accept all changes without prompting")]
        bool autoYes) : base(activator)
    {
        _tableInfo = table;
        _alsoSyncAno = alsoSyncAno;
        _autoYes = autoYes;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.Sync);
    }

    public override string GetCommandName()
    {
        return _alsoSyncAno ? "Sync TableInfo and ANO Configuration" : "Sync TableInfo";
    }

    public override void Execute()
    {
        base.Execute();

        var syncher = new TableInfoSynchronizer(_tableInfo);
        var listener = _autoYes
            ? new AcceptAllCheckNotifier()
            : (ICheckNotifier)new FromActivateItemsToCheckNotifier(BasicActivator);

        try
        {
            var wasSynchedsuccessfully = syncher.Synchronize(listener);

            BasicActivator.Show(wasSynchedsuccessfully
                ? "Synchronization complete, TableInfo is Synchronized with the live database"
                : "Synchronization failed");
        }
        catch (Exception exception)
        {
            BasicActivator.ShowException("Failed to sync", exception);
        }

        if (_alsoSyncAno)
        {
            var ANOSynchronizer = new ANOTableInfoSynchronizer(_tableInfo);

            try
            {
                ANOSynchronizer.Synchronize(listener);

                BasicActivator.Show("ANO synchronization successful");
            }
            catch (ANOConfigurationException e)
            {
                BasicActivator.ShowException("Anonymisation configuration error", e);
            }
            catch (Exception exception)
            {
                BasicActivator.ShowException($"Fatal error while attempting to synchronize ({exception.Message})",
                    exception);
            }
        }

        Publish(_tableInfo);

        foreach (var c in syncher.ChangedCatalogues)
            Publish(c);
    }
}