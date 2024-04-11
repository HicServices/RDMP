// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Providers;
using Rdmp.Core.Sharing.Dependency.Gathering;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Allows you to perform database wide find and replace operations.  This is a useful but very dangerous feature, it is possible to very easily break your Catalogue.  The
/// feature is primarily intended for system wide operations such as when you change a network UNC folder location or mapped network drive and you need to change ALL references
/// to the root path to the new location.
/// 
/// <para>Sql properties are also exposed but this is even more dangerous to modify.  For example if you change a database name and want to perform a system wide rename on all
/// references including filters, extractable columns, extracted project definitions etc etc</para>
/// 
/// <para>You should always back up both your Catalogue and DataExport databases before embarking on a Find and Replace</para>
/// </summary>
public partial class FindAndReplaceUI : RDMPUserControl
{
    private HashSet<IMapsDirectlyToDatabaseTable> _allObjects = new();

    private IAttributePropertyFinder _adjustableLocationPropertyFinder;
    private List<FindAndReplaceNode> _locationNodes = new();

    private IAttributePropertyFinder _sqlPropertyFinder;
    private List<FindAndReplaceNode> _sqlNodes = new();


    public FindAndReplaceUI(IActivateItems activator)
    {
        SetItemActivator(activator);

        GetAllObjects(activator);

        _adjustableLocationPropertyFinder = new AttributePropertyFinder<AdjustableLocationAttribute>(_allObjects);
        _sqlPropertyFinder = new AttributePropertyFinder<SqlAttribute>(_allObjects);

        InitializeComponent();

        olvObject.ImageGetter += ImageGetter;
        olvProperty.AspectGetter += PropertyAspectGetter;
        olvValue.AspectGetter += ValueAspectGetter;

        olvAllObjects.AlwaysGroupByColumn = olvProperty;

        //allow editing
        olvAllObjects.CellEditFinished += OlvAllObjectsCellEditFinished;

        //Create all the nodes up front
        foreach (var o in _allObjects.Where(_adjustableLocationPropertyFinder.ObjectContainsProperty))
            foreach (var propertyInfo in _adjustableLocationPropertyFinder.GetProperties(o))
                _locationNodes.Add(new FindAndReplaceNode(o, propertyInfo));

        foreach (var o in _allObjects.Where(_sqlPropertyFinder.ObjectContainsProperty))
            foreach (var propertyInfo in _sqlPropertyFinder.GetProperties(o))
                _sqlNodes.Add(new FindAndReplaceNode(o, propertyInfo));

        olvAllObjects.BeginUpdate();
        olvAllObjects.AddObjects(_locationNodes);
        olvAllObjects.EndUpdate();
    }

    private void GetAllObjects(IActivateItems activator)
    {
        var g = new Gatherer(activator.RepositoryLocator);

        //We get these from the child provider because some objects (those below go off looking stuff up if you get them
        //and do not inject known good values first)
        foreach (var o in Activator.CoreChildProvider.AllExtractionInformations.Value)
            _allObjects.Add(o);

        foreach (var o in Activator.CoreChildProvider.AllCatalogueItems.Value)
            _allObjects.Add(o);

        if (Activator.CoreChildProvider is DataExportChildProvider dxmChildProvider)
            foreach (var o in dxmChildProvider.GetAllExtractableColumns(
                         Activator.RepositoryLocator.DataExportRepository))
                _allObjects.Add(o);

        foreach (var o in g.GetAllObjectsInAllDatabases())
            _allObjects.Add(o);
    }

    private void OlvAllObjectsCellEditFinished(object sender, CellEditEventArgs e)
    {
        if (e?.RowObject == null)
            return;

        var node = (FindAndReplaceNode)e.RowObject;
        node.SetValue(e.NewValue);
        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs((DatabaseEntity)node.Instance));
    }

    private object ValueAspectGetter(object rowobject) => ((FindAndReplaceNode)rowobject).GetCurrentValue();

    private object PropertyAspectGetter(object rowobject)
    {
        var node = (FindAndReplaceNode)rowobject;

        return node.PropertyName;
    }

    private Bitmap ImageGetter(object rowObject) => rowObject == null
        ? null
        : Activator.CoreIconProvider.GetImage(((FindAndReplaceNode)rowObject).Instance).ImageToBitmap();


    private void CheckedChanged(object sender, EventArgs e)
    {
        var cb = (RadioButton)sender;

        olvAllObjects.BeginUpdate();
        if (cb.Checked)
        {
            olvAllObjects.ClearObjects();
            olvAllObjects.SuspendLayout();

            olvAllObjects.AddObjects(sender == rbLocationsAttribute ? _locationNodes : _sqlNodes);

            olvAllObjects.ResumeLayout();
        }

        olvAllObjects.EndUpdate();
    }

    private void btnReplaceAll_Click(object sender, EventArgs e)
    {
        if (Activator.YesNo(
                "Are you sure you want to do a system wide find and replace? This operation cannot be undone",
                "Are you sure"))
            foreach (FindAndReplaceNode node in olvAllObjects.FilteredObjects)
                node.FindAndReplace(tbFind.Text, tbReplace.Text, !cbMatchCase.Checked);
    }

    private void tlvAllObjects_ItemActivate(object sender, EventArgs e)
    {
        if (olvAllObjects.SelectedObject is not FindAndReplaceNode node) return;
        var cmd = new ExecuteCommandActivate(Activator, node.Instance);
        if (!cmd.IsImpossible)
            cmd.Execute();
    }

    private TextMatchFilter _textMatchFilter;

    private void btnFind_Click(object sender, EventArgs e)
    {
        if (olvAllObjects.ModelFilter is not CompositeAllFilter all)
            olvAllObjects.ModelFilter = all = new CompositeAllFilter(new List<IModelFilter>());

        if (_textMatchFilter != null && all.Filters.Contains(_textMatchFilter))
            all.Filters.Remove(_textMatchFilter);

        _textMatchFilter = new TextMatchFilter(olvAllObjects, tbFind.Text,
            cbMatchCase.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

        all.Filters.Add(_textMatchFilter);

        olvAllObjects.ModelFilter = all;
    }
}