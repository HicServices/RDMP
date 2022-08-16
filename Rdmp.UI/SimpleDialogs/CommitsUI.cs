using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.SqlDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class CommitsUI : RDMPForm
    {
        private List<Commit> _commits;

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
