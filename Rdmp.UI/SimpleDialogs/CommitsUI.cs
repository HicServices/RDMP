// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.SqlDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs;

public partial class CommitsUI : CommitsUI_Design
{
    private RDMPCollectionCommonFunctionality CommonCollectionFunctionality = new();

    public const string GeneralAdviceAboutWhatIsShown =
        "Only includes changes made while 'Commit' system was enabled.  Does not include changes made by processes/commands that do not support Commit system.";

    private CommitsUI()
    {
        InitializeComponent();

        treeListView1.FullRowSelect = true;
        treeListView1.ItemActivate += TreeListView1_ItemActivate;
        treeListView1.CanExpandGetter = static m => m is Commit;
        treeListView1.ChildrenGetter = static m => m is Commit c ? c.Mementos : null;
    }

    /// <summary>
    /// Creates a new instance showing all <see cref="Commit"/> ever made regardless of what object
    /// they were working on.
    /// </summary>
    /// <param name="activator"></param>
    public CommitsUI(IActivateItems activator) : this()
    {
        SetItemActivator(activator);

        SetupCommonCollectionFunctionality();

        var commits = activator.RepositoryLocator.CatalogueRepository
            .GetAllObjects<Commit>()
            .ToList();

        treeListView1.AddObjects(commits);
        taskDescriptionLabel1.SetupFor(new DialogArgs
        {
            TaskDescription = $"Showing all commits. {GeneralAdviceAboutWhatIsShown}"
        });
    }

    public CommitsUI(IActivateItems activator, IMapsDirectlyToDatabaseTable o) : this()
    {
        SetItemActivator(activator);

        SetupCommonCollectionFunctionality();

        // TODO: move this to a helper class
        var commitsInvolvingObject = activator.RepositoryLocator.CatalogueRepository
            .GetAllObjectsWhere<Memento>(nameof(Memento.ReferencedObjectID), o.ID)
            .Where(m => m.IsReferenceTo(o))
            .Select(m => m.Commit_ID)
            .Distinct()
            .ToList();

        var commits = activator.RepositoryLocator.CatalogueRepository
            .GetAllObjectsInIDList<Commit>(commitsInvolvingObject)
            .ToList();

        treeListView1.AddObjects(commits);
        taskDescriptionLabel1.SetupFor(new DialogArgs
        {
            TaskDescription = $"Showing all commits that involved changes to '{o}'. {GeneralAdviceAboutWhatIsShown}"
        });
    }

    private void SetupCommonCollectionFunctionality()
    {
        CommonCollectionFunctionality.SetUp(Core.RDMPCollection.None, treeListView1, Activator, olvName, null,
            new RDMPCollectionCommonFunctionalitySettings
            {
                SuppressActivate = true,
                SuppressChildrenAdder = true,
                AddCheckColumn = false,
                AddIDColumn = false,
                AddFavouriteColumn = false
            });

        RDMPCollectionCommonFunctionality.SetupColumnSortTracking(treeListView1,
            new Guid("6199b509-a52a-4d38-ae47-023dbe891c7d"));

        CommonCollectionFunctionality.SetupColumnTracking(olvName, new Guid("c502c0bb-eda6-4703-b6bd-8124d7792a57"));
        CommonCollectionFunctionality.SetupColumnTracking(olvDate, new Guid("b438cb32-9610-4bc8-8590-9288e1cc0ef7"));
        CommonCollectionFunctionality.SetupColumnTracking(olvUser, new Guid("c544ad0a-4ccf-4874-8269-b64465c6010f"));
        CommonCollectionFunctionality.SetupColumnTracking(olvDescription,
            new Guid("a53f80a5-c0a2-40c0-a8ec-1d3a897fcce4"));
    }

    private void TreeListView1_ItemActivate(object sender, EventArgs e)
    {
        if (treeListView1.SelectedObject is not Memento m)
            return;

        var dialog = new SQLBeforeAndAfterViewer(m.BeforeYaml, m.AfterYaml, "Before", "After", m.ToString(),
            MessageBoxButtons.OK);
        dialog.Show();
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbFilter.Text))
        {
            treeListView1.ModelFilter = null;
        }
        else
        {
            treeListView1.ModelFilter = new TextMatchFilter(treeListView1, tbFilter.Text)
            { StringComparison = StringComparison.CurrentCultureIgnoreCase };
            treeListView1.UseFiltering = true;
        }
    }

    private void btnExpandAll_Click(object sender, EventArgs e)
    {
        treeListView1.ExpandAll();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CommitsUI_Design, UserControl>))]
public abstract class CommitsUI_Design : RDMPUserControl;