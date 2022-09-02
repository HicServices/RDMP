using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.SqlDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class CommitsUI : RDMPForm
    {
        private List<Commit> _commits;

        RDMPCollectionCommonFunctionality CommonCollectionFunctionality = new ();


        public CommitsUI(IActivateItems activator, IMapsDirectlyToDatabaseTable o)
        {
            InitializeComponent();

            SetItemActivator(activator);

            // TODO: move this to a helper class
            var commitsInvolvingObject = activator.RepositoryLocator.CatalogueRepository
                .GetAllObjectsWhere<Memento>(nameof(Memento.ReferencedObjectID), o.ID)
                .Where((m) => m.IsReferenceTo(o))
                .Select(m=>m.Commit_ID)
                .Distinct()
                .ToList();

            _commits = activator.RepositoryLocator.CatalogueRepository
                .GetAllObjectsInIDList<Commit>(commitsInvolvingObject)
                .ToList();

            treeListView1.FullRowSelect = true;
            treeListView1.ItemActivate += TreeListView1_ItemActivate;
            treeListView1.AddObjects(_commits);
            treeListView1.CanExpandGetter = (m) => m is Commit;
            treeListView1.ChildrenGetter = (m) => m is Commit c ? c.Mementos : null;

            CommonCollectionFunctionality.SetUp(Core.RDMPCollection.None, treeListView1, activator, olvName, null, new RDMPCollectionCommonFunctionalitySettings
            {
                SuppressActivate = true,
                SuppressChildrenAdder = true,
                AddCheckColumn = false,
                AddIDColumn = false,
                AddFavouriteColumn = false
            });

            RDMPCollectionCommonFunctionality.SetupColumnSortTracking(treeListView1, new Guid("6199b509-a52a-4d38-ae47-023dbe891c7d"));

            CommonCollectionFunctionality.SetupColumnTracking(olvName, new Guid("c502c0bb-eda6-4703-b6bd-8124d7792a57"));
            CommonCollectionFunctionality.SetupColumnTracking(olvDate, new Guid("b438cb32-9610-4bc8-8590-9288e1cc0ef7"));
            CommonCollectionFunctionality.SetupColumnTracking(olvUser, new Guid("c544ad0a-4ccf-4874-8269-b64465c6010f"));
            CommonCollectionFunctionality.SetupColumnTracking(olvDescription, new Guid("a53f80a5-c0a2-40c0-a8ec-1d3a897fcce4"));

        }

        private void TreeListView1_ItemActivate(object sender, System.EventArgs e)
        {
            if(treeListView1.SelectedObject is not Memento m)
                return;

            var dialog = new SQLBeforeAndAfterViewer(m.BeforeYaml,m.AfterYaml,"Before","After",m.ToString(),MessageBoxButtons.OK);
            dialog.Show();
        }
    }
}
