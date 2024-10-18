// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleGuiContextMenuFactory
{
    private IBasicActivateItems activator;

    public ConsoleGuiContextMenuFactory(IBasicActivateItems activator)
    {
        this.activator = activator;
    }

    public ContextMenu Create(int x,int y,object[] many, object single)
    {
        var commands = GetCommands(activator, many, single).ToArray();

        var order = new Dictionary<MenuItem, float>();

        // Build subcategories
        var categories = commands
            .OrderBy(c => c.Weight)
            .Select(c => c.SuggestedCategory)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct();

        var miCategories = categories.ToDictionary(category => category, _ => new List<MenuItem>());

        var items = new List<MenuItem>();

        // Build commands into menu items
        foreach (var cmd in commands.OrderBy(c => c.Weight))
        {
            var item = new MenuItem(cmd.GetCommandName(), null, () => ExecuteWithCatch(cmd));
            order.Add(item, cmd.Weight);

            if (cmd.SuggestedCategory != null)
                miCategories[cmd.SuggestedCategory].Add(item);
            else
                items.Add(item);
        }

        foreach (var kvp in miCategories)
        {
            // menu bar order is the minimum of the menu items in it
            var bar = new MenuBarItem(kvp.Key, AddSpacers(kvp.Value, order));
            order.Add(bar, kvp.Value.Select(m => order[m]).Min());
            items.Add(bar);
        }

        // we can do nothing if there are no menu items
        if (items.Count == 0)
            return null;

        var withSpacers = AddSpacers(items, order);

        return new ContextMenu(x, y, new MenuBarItem(withSpacers))
        {
            UseSubMenusSingleFrame = true
        };
    }

    private static MenuItem[] AddSpacers(List<MenuItem> items, Dictionary<MenuItem, float> order)
    {
        // sort it
        items.OrderBy(m => order[m]).ToList();

        // add spacers when the Weight differs by more than 1 whole number
        var withSpacers = new List<MenuItem>();
        var lastWeightSeen = (int)order[items.First()];

        foreach (var item in items)
        {
            if (lastWeightSeen != (int)order[item])
            {
                // add a spacer
                withSpacers.Add(null);
                lastWeightSeen = (int)order[item];
            }

            withSpacers.Add(item);
        }

        return withSpacers.ToArray();
    }

    private void ExecuteWithCatch(IAtomicCommand cmd)
    {
        try
        {
            cmd.Execute();
        }
        catch (Exception ex)
        {
            activator.ShowException($"Error running command '{cmd.GetCommandName()}'", ex);
        }
    }

    private static IEnumerable<IAtomicCommand> GetCommands(IBasicActivateItems activator, object[] many, object single)
    {
        var factory = new AtomicCommandFactory(activator);

        if (many.Length > 1) return factory.CreateManyObjectCommands(many).ToArray();

        var o = single;

        if (ReferenceEquals(o, ConsoleMainWindow.Catalogues))
            return new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewCatalogueByImportingFile(activator),
                new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(activator)
            };
        if (ReferenceEquals(o, ConsoleMainWindow.Loads))
            return new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewLoadMetadata(activator)
            };
        if (ReferenceEquals(o, ConsoleMainWindow.Projects))
            return new IAtomicCommand[]
            {
                new ExecuteCommandNewObject(activator, typeof(Project)) { OverrideCommandName = "New Project" }
            };
        if (ReferenceEquals(o, ConsoleMainWindow.CohortConfigs))
            return new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewCohortIdentificationConfiguration(activator)
            };

        return o == null
            ? Array.Empty<IAtomicCommand>()
            : GetExtraCommands(activator, o)
                .Union(factory.CreateCommands(o))
                .Union(activator.PluginUserInterfaces.SelectMany(p => p.GetAdditionalRightClickMenuItems(o)))
                .OrderBy(c => c.Weight);
    }

    private static IEnumerable<IAtomicCommand> GetExtraCommands(IBasicActivateItems activator, object o)
    {
        if (CommandFactoryBase.Is(o, out LoadMetadata lmd))
            yield return new ExecuteCommandRunConsoleGuiView(activator,
                    () => new RunDleWindow(activator, lmd))
            { OverrideCommandName = "Execute Load..." };

        if (CommandFactoryBase.Is(o, out Project p))
            yield return new ExecuteCommandRunConsoleGuiView(activator,
                    () => new RunReleaseWindow(activator, p))
            { OverrideCommandName = "Release..." };
        if (CommandFactoryBase.Is(o, out ExtractionConfiguration ec))
        {
            yield return new ExecuteCommandRunConsoleGuiView(activator,
                    () => new RunReleaseWindow(activator, ec))
            { OverrideCommandName = "Release..." };

            yield return new ExecuteCommandRunConsoleGuiView(activator,
                    () => new RunExtractionWindow(activator, ec))
            { OverrideCommandName = "Extract..." };
        }

        if (CommandFactoryBase.Is(o, out CacheProgress cp))
            yield return new ExecuteCommandRunConsoleGuiView(activator,
                    () => new RunCacheWindow(activator, cp))
            { OverrideCommandName = "Run Cache..." };

        if (CommandFactoryBase.Is(o, out Catalogue c) && !c.IsApiCall())
            yield return new ExecuteCommandRunConsoleGuiView(activator,
                    () => new RunDataQualityEngineWindow(activator, c))
            { OverrideCommandName = "Run DQE..." };
    }
}