using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    internal class AllDataAccessCredentialsNodeMenu : RDMPContextMenuStrip
    {
        public AllDataAccessCredentialsNodeMenu(RDMPContextMenuStripArgs args, AllDataAccessCredentialsNode node): base(args, node)
        {
            Items.Add("Add New Credentials", _activator.CoreIconProvider.GetImage(RDMPConcept.DataAccessCredentials,OverlayKind.Add), (s, e) => AddCredentials());
        }

        private void AddCredentials()
        {
            var newCredentials = new DataAccessCredentials(RepositoryLocator.CatalogueRepository, "New Blank Credentials " + Guid.NewGuid());

            Publish(newCredentials);
        }
    }
}