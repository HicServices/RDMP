// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Menus;

/// <summary>
/// Base class for all right click context menus in <see cref="RDMPCollectionUI"/> controls.  These menus are built by reflection
/// when the selected object is changed.
/// </summary>
[DesignerCategory("")]
public class RDMPContextMenuStrip : ContextMenuStrip
{
    private readonly object _o;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }

    protected IActivateItems _activator;

    protected readonly AtomicCommandUIFactory AtomicCommandUIFactory;

    protected ToolStripMenuItem ActivateCommandMenuItem;
    private RDMPContextMenuStripArgs _args;

    private Dictionary<string, ToolStripMenuItem> _subMenuDictionary = new();

    public const string Checks = "Run Checks";
    public const string Tree = "Tree";
    public const string Alter = "Alter";

    public RDMPContextMenuStrip(RDMPContextMenuStripArgs args, object o)
    {
        _o = o;
        _args = args;

        //we will add this ourselves in AddCommonMenuItems
        _args.SkipCommand<ExecuteCommandActivate>();

        _activator = _args.ItemActivator;

        _activator.Theme.ApplyTo(this);

        AtomicCommandUIFactory = new AtomicCommandUIFactory(_activator);

        RepositoryLocator = _activator.RepositoryLocator;

        if (o is not null and not RDMPCollection)
        {
            var activateCommand = new ExecuteCommandActivate(_activator, args.Masquerader ?? o);
            ActivateCommandMenuItem = Add(activateCommand, Keys.None);

            if (activateCommand.ReasonCommandImpossible?.Equals(GlobalStrings.ObjectCannotBeActivated) ?? false)
                Items.Remove(ActivateCommandMenuItem);
        }
    }

    /// <summary>
    /// Register an event for opening the supplied dropdown that fetches all lazy objects and updates command viability of subitems (see <see cref="ExecuteCommandShow.FetchDestinationObjects"/>
    /// </summary>
    /// <param name="gotoMenu"></param>
    public static void RegisterFetchGoToObjecstCallback(ToolStripMenuItem gotoMenu)
    {
        gotoMenu.DropDownOpening += (s, e) =>
        {
            foreach (var mi in gotoMenu.DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (mi.Tag is ExecuteCommandShow cmd)
                {
                    cmd.FetchDestinationObjects();
                    mi.Enabled = !cmd.IsImpossible;
                    mi.ToolTipText = cmd.ReasonCommandImpossible;
                }

                if (mi.Tag is ExecuteCommandSimilar cmdSimilar)
                {
                    cmdSimilar.FetchMatches();
                    mi.Enabled = !cmdSimilar.IsImpossible;
                    mi.ToolTipText = cmdSimilar.ReasonCommandImpossible;
                }
            }
        };
    }

    protected void ReBrandActivateAs(string newTextForActivate, RDMPConcept newConcept,
        OverlayKind overlayKind = OverlayKind.None)
    {
        // if we are rebranding activate let's make sure its definitely in the menu
        if (!Items.Contains(ActivateCommandMenuItem)) Items.Insert(0, ActivateCommandMenuItem);
        ActivateCommandMenuItem.Image = _activator.CoreIconProvider.GetImage(newConcept, overlayKind).ImageToBitmap();
        ActivateCommandMenuItem.Text = newTextForActivate;
    }


    protected ToolStripMenuItem Add(IAtomicCommand cmd, Keys shortcutKey = Keys.None, ToolStripMenuItem toAddTo = null)
    {
        var mi = AtomicCommandUIFactory.CreateMenuItem(cmd);

        if (shortcutKey != Keys.None)
            mi.ShortcutKeys = shortcutKey;

        if (toAddTo == null)
            Items.Add(mi);
        else
            toAddTo.DropDownItems.Add(mi);

        return mi;
    }

    /// <summary>
    /// Creates a new command under a submenu named <paramref name="submenu"/> (if this doesn't exist yet it will be created).
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="shortcutKey"></param>
    /// <param name="submenu"></param>
    /// <param name="image"></param>
    protected void Add(IAtomicCommand cmd, Keys shortcutKey, string submenu, Image image = null)
    {
        Add(cmd, shortcutKey, AddMenuIfNotExists(submenu, image));
    }


    private ToolStripMenuItem AddMenuIfNotExists(string submenu, Image image = null)
    {
        if (!_subMenuDictionary.TryGetValue(submenu, out var menuItem))
        {
            menuItem = new ToolStripMenuItem(submenu, image);
            _subMenuDictionary.Add(submenu, menuItem);
        }

        return menuItem;
    }

    public void AddCommonMenuItems(RDMPCollectionCommonFunctionality commonFunctionality)
    {
        AddFactoryMenuItems();

        var databaseEntity = _o as DatabaseEntity;

        var treeMenuItem = AddMenuIfNotExists(Tree);

        if (_o is IMapsDirectlyToDatabaseTable m) Add(new ExecuteCommandViewCommits(_activator, m));

        //ensure all submenus appear in the same place
        foreach (var mi in _subMenuDictionary.Values)
            Items.Add(mi);

        //add plugin menu items
        foreach (var plugin in _activator.PluginUserInterfaces)
            try
            {
                foreach (var cmd in plugin.GetAdditionalRightClickMenuItems(
                             _o is RDMPCollectionCommonFunctionality c ? c.Collection : _o))
                    Add(cmd);
            }
            catch (Exception ex)
            {
                _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(
                    $"Plugin '{plugin.GetType().Name}' failed in call to 'GetAdditionalRightClickMenuItems':{Environment.NewLine}{ex.Message}",
                    CheckResult.Fail, ex));
            }

        //Check if we even want to display this
        if (commonFunctionality.CheckColumnProvider != null)
        {
            var inspectionMenuItem = AddMenuIfNotExists(Checks);
            Items.Add(inspectionMenuItem);
            PopulateChecksMenu(commonFunctionality, inspectionMenuItem);
        }

        //add seldom used submenus (pin, view dependencies etc)
        Items.Add(treeMenuItem);
        PopulateTreeMenu(commonFunctionality, treeMenuItem);

        if (databaseEntity != null)
        {
            Add(new ExecuteCommandAddFavourite(_activator, databaseEntity));
            Add(new ExecuteCommandAddToSession(_activator, new IMapsDirectlyToDatabaseTable[] { databaseEntity },
                null));
        }

        //add refresh and then finally help
        if (databaseEntity != null)
            Add(new ExecuteCommandRefreshObject(_activator, databaseEntity), Keys.F5);

        Add(new ExecuteCommandShowKeywordHelp(_activator, _args));

        var gotoMenu = Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text.Equals(AtomicCommandFactory.GoTo));

        if (gotoMenu != null)
            RegisterFetchGoToObjecstCallback(gotoMenu);

        //ensure any new submenus still appear
        foreach (var mi in _subMenuDictionary.Values.Except(Items.OfType<ToolStripMenuItem>()))
            Items.Add(mi);
    }

    private void AddFactoryMenuItems()
    {
        var factory = new AtomicCommandFactory(_activator);

        var start = DateTime.Now;
        var now = DateTime.Now;
        var performance = new Dictionary<IAtomicCommand, TimeSpan>();

        var forObject = _args.Masquerader ?? _o;
        foreach (var toPresent in factory.CreateCommands(forObject))
        {
            // how long did it take to construct the command?
            performance.Add(toPresent, DateTime.Now.Subtract(now));

            Add(toPresent);
            now = DateTime.Now;
        }

        if (UserSettings.DebugPerformance)
        {
            var timings = string.Join(Environment.NewLine,
                performance.Select(kvp => $"{kvp.Key}:{kvp.Value.TotalMilliseconds}ms"));

            _activator.GlobalErrorCheckNotifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Creating menu for '{forObject}' took {DateTime.Now.Subtract(start).Milliseconds}ms:{Environment.NewLine}{timings}",
                    CheckResult.Success));
        }
    }

    public void Add(IAtomicCommand toPresent)
    {
        if (_args.ShouldSkipCommand(toPresent))
            return;

        var key = Keys.None;

        if (!string.IsNullOrWhiteSpace(toPresent.SuggestedShortcut))
            Enum.TryParse<Keys>(toPresent.SuggestedShortcut, out key);

        if (toPresent.SuggestedCategory == null)
            Add(toPresent, toPresent.Ctrl ? Keys.Control | key : key);
        else
            Add(toPresent, toPresent.Ctrl ? Keys.Control | key : key, toPresent.SuggestedCategory);
    }

    private void PopulateTreeMenu(RDMPCollectionCommonFunctionality commonFunctionality, ToolStripMenuItem treeMenuItem)
    {
        if (_args.Tree != null && !commonFunctionality.Settings.SuppressChildrenAdder)
        {
            Add(new ExecuteCommandExpandAllNodes(_activator, commonFunctionality, _args.Model), Keys.None,
                treeMenuItem);
            Add(new ExecuteCommandCollapseChildNodes(_activator, commonFunctionality, _args.Model), Keys.None,
                treeMenuItem);
        }

        treeMenuItem.Enabled = treeMenuItem.HasDropDown;
    }

    private void PopulateChecksMenu(RDMPCollectionCommonFunctionality commonFunctionality,
        ToolStripMenuItem inspectionMenuItem)
    {
        if (commonFunctionality.CheckColumnProvider != null)
        {
            if (_o is DatabaseEntity databaseEntity)
                Add(
                    new ExecuteCommandCheckAsync(_activator, databaseEntity,
                        commonFunctionality.CheckColumnProvider.RecordWorst), Keys.None, inspectionMenuItem);

            var checkAll = new ToolStripMenuItem("Check All", null,
                (s, e) => commonFunctionality.CheckColumnProvider.CheckCheckables())
            {
                /* The Weight of ExecuteCommandCheckAsync to ensure there is no tool strip separator*/
                Tag = 100.4f,
                ToolTipText = "Run validation checks for all visible items in the current window",
                Image = CatalogueIcons.TinyYellow.ImageToBitmap(),
                Enabled = commonFunctionality.CheckColumnProvider.GetCheckables().Any()
            };
            inspectionMenuItem.DropDownItems.Add(checkAll);
        }

        // disable menu if checking is not supported in the collection or objects clicked are not checkable
        inspectionMenuItem.Enabled = inspectionMenuItem.HasDropDown &&
                                     inspectionMenuItem.DropDownItems.OfType<ToolStripMenuItem>().Any(m => m.Enabled);
    }

    protected void Activate(DatabaseEntity o)
    {
        var cmd = new ExecuteCommandActivate(_activator, o);
        cmd.Execute();
    }

    protected void Publish(DatabaseEntity o)
    {
        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(o));
    }

    protected Image GetImage(object concept, OverlayKind shortcut = OverlayKind.None) =>
        _activator.CoreIconProvider.GetImage(concept, shortcut).ImageToBitmap();

    protected void Emphasise(DatabaseEntity o, int expansionDepth = 0)
    {
        _activator.RequestItemEmphasis(this, new EmphasiseRequest(o, expansionDepth));
    }
}