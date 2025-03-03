// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Collections;

/// <summary>
/// Toolbox control for storing user defined collections of objects.  Similar to <see cref="FavouritesCollectionUI"/> but for limited lifetime scope.  Note that this class inherits from <see cref="RDMPUserControl"/> not <see cref="RDMPCollectionUI"/>
/// </summary>
public class SessionCollectionUI : RDMPUserControl, IObjectCollectionControl, IConsultableBeforeClosing
{
    private BrightIdeasSoftware.TreeListView olvTree;
    private BrightIdeasSoftware.OLVColumn olvName;
    private bool _firstTime = true;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SessionCollection Collection { get; private set; }

    public RDMPCollectionCommonFunctionality CommonTreeFunctionality { get; } = new();

    public SessionCollectionUI()
    {
        InitializeComponent();

        olvName.AspectGetter = o => o.ToString();
        CommonTreeFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration) };
    }

    public IPersistableObjectCollection GetCollection() => Collection;

    public string GetTabName() => Collection?.SessionName ?? "Unnamed Session";

    public string GetTabToolTip() => null;

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        SetItemActivator(activator);

        Collection = (SessionCollection)collection;

        if (!CommonTreeFunctionality.IsSetup)
            CommonTreeFunctionality.SetUp(RDMPCollection.None, olvTree, activator, olvName, olvName,
                new RDMPCollectionCommonFunctionalitySettings
                {
                    // add custom options here
                });

        RefreshSessionObjects();

        CommonFunctionality.ClearToolStrip();
        CommonFunctionality.Add(new ToolStripButton("Add...", null, AddObjectToSession));
        CommonFunctionality.Add(new ToolStripButton("Remove...", null, RemoveObjectsFromSession));

        if (_firstTime)
        {
            CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("a6abe085-f5cc-4ce0-85ef-0d42e7dbfced"));
            _firstTime = false;
        }
    }

    private void RemoveObjectsFromSession(object sender, EventArgs e)
    {
        var toRemove = Activator.SelectMany("Remove From Session", typeof(IMapsDirectlyToDatabaseTable),
            Collection.DatabaseObjects.ToArray());

        if (toRemove is { Length: > 0 }) Remove(toRemove);
    }

    /// <summary>
    /// Adds <paramref name="toAdd"/> to the list of objects tracked in the session
    /// </summary>
    /// <param name="toAdd"></param>
    public void Add(params IMapsDirectlyToDatabaseTable[] toAdd)
    {
        for (var i = 0; i < toAdd.Length; i++)
            toAdd[i] = toAdd[i] switch
            {
                //unwrap pipelines
                PipelineCompatibleWithUseCaseNode pcn => pcn.Pipeline,
                SpontaneousObject => throw new NotSupportedException("Object cannot be added to sessions"),
                _ => toAdd[i]
            };

        Collection.DatabaseObjects = toAdd.Union(Collection.DatabaseObjects).ToList();
        RefreshSessionObjects();
    }

    /// <summary>
    /// Removes <paramref name="toRemove"/> from the list of objects tracked in the session
    /// </summary>
    /// <param name="toRemove"></param>
    public void Remove(params IMapsDirectlyToDatabaseTable[] toRemove)
    {
        foreach (var r in toRemove)
        {
            Collection.DatabaseObjects.Remove(r);
            olvTree.RemoveObject(r);
        }

        RefreshSessionObjects();
    }

    private void AddObjectToSession(object sender, EventArgs e)
    {
        var toAdd = Activator.SelectMany(new DialogArgs
        {
            WindowTitle = "Add to Session",
            TaskDescription = "Pick which objects you want added to the session window."
        }, typeof(IMapsDirectlyToDatabaseTable),
                Activator.CoreChildProvider.GetAllSearchables().Keys.Except(Collection.DatabaseObjects).ToArray())
            ?.ToList();

        if (toAdd == null || toAdd.Count == 0)
            // user cancelled picking objects
            return;

        Add(toAdd.ToArray());
    }

    private void RefreshSessionObjects()
    {
        var actualObjects = FavouritesCollectionUI.FindRootObjects(Activator, Collection.DatabaseObjects.Contains)
            .Union(Collection.DatabaseObjects.OfType<Pipeline>()).ToList();

        //no change in root favouritism
        if (actualObjects.SequenceEqual(olvTree.Objects.OfType<IMapsDirectlyToDatabaseTable>()))
            return;

        //remove old objects
        foreach (var old in Collection.DatabaseObjects.Except(actualObjects))
            olvTree.RemoveObject(old);

        //add new objects
        foreach (var newObject in actualObjects.Except(olvTree.Objects.OfType<IMapsDirectlyToDatabaseTable>()))
            olvTree.AddObject(newObject);

        //update to the new list
        Collection.DatabaseObjects = actualObjects;
        olvTree.RebuildAll(true);
    }

    public override string ToString() => Collection?.SessionName ?? "Unnamed Session";

    #region InitializeComponent

    private void InitializeComponent()
    {
        olvTree = new BrightIdeasSoftware.TreeListView();
        olvName = new BrightIdeasSoftware.OLVColumn();
        ((ISupportInitialize)olvTree).BeginInit();
        SuspendLayout();
        // 
        // olvRecent
        // 
        olvTree.AllColumns.Add(olvName);
        olvTree.CellEditUseWholeCell = false;
        olvTree.Columns.AddRange(new ColumnHeader[]
        {
            olvName
        });
        olvTree.Cursor = Cursors.Default;
        olvTree.Dock = DockStyle.Fill;
        olvTree.FullRowSelect = true;
        olvTree.HideSelection = false;
        olvTree.Location = new System.Drawing.Point(0, 0);
        olvTree.Name = "olvRecent";
        olvTree.RowHeight = 19;
        olvTree.ShowGroups = false;
        olvTree.Size = new System.Drawing.Size(487, 518);
        olvTree.TabIndex = 4;
        olvTree.UseCompatibleStateImageBehavior = false;
        olvTree.View = View.Details;
        olvTree.VirtualMode = true;
        // 
        // olvName
        // 
        olvName.Groupable = false;
        olvName.Text = "Name";
        // 
        // SessionCollectionUI
        // 
        Controls.Add(olvTree);
        Name = "SessionCollectionUI";
        Size = new System.Drawing.Size(487, 518);
        ((ISupportInitialize)olvTree).EndInit();
        ResumeLayout(false);
    }

    public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
            e.Cancel = !Activator.YesNo($"Close Session {Collection.SessionName}? (this will end the session)",
                "End Session");
    }

    #endregion
}