using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPStartup;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    internal class AllDataAccessCredentialsNodeMenu : RDMPContextMenuStrip
    {
        public AllDataAccessCredentialsNodeMenu(IActivateItems activator, AllDataAccessCredentialsNode node):base(activator,null)
        {
            Items.Add("Add New Credentials", activator.CoreIconProvider.GetImage(RDMPConcept.DataAccessCredentials,OverlayKind.Add), (s, e) => AddCredentials());
        }

        private void AddCredentials()
        {
            var newCredentials = new DataAccessCredentials(RepositoryLocator.CatalogueRepository, "New Blank Credentials " + Guid.NewGuid());

            Publish(newCredentials);
        }
    }
}