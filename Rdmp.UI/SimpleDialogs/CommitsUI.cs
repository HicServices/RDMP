using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
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

            treeListView1.AddObjects(_commits);
        }
    }
}
