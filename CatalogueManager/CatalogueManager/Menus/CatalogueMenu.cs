using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Cloning;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.AggregationUIs;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.DataLoadUIs;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Issues;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.ObjectVisualisation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Validation;
using DataLoadEngine.Migration;
using DataQualityEngine.Data;
using DataQualityEngine.Reports;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Dependencies;
using CatalogueLibrary.Data;
using ReusableUIComponents.Icons.IconProvision;
using ReusableUIComponents.Progress;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class CatalogueMenu:RDMPContextMenuStrip
    {
        public CatalogueMenu(IActivateItems activator, CatalogueFolder folder, RDMPCollectionCommonFunctionality collection) : base(activator,null, collection)
        {
            AddImportOptions();
            AddCommonMenuItems(folder);
        }

        public CatalogueMenu(IActivateItems activator, Catalogue catalogue, RDMPCollectionCommonFunctionality collection):base(activator,catalogue, collection)
        {
            //create right click context menu
            Add(new ExecuteCommandViewCatalogueExtractionSql(_activator).SetTarget(catalogue));

            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks(catalogue));

            Items.Add(new ToolStripSeparator());

            var addItem = new ToolStripMenuItem("Add", null,

                new AddSupportingSqlTableMenuItem(_activator, catalogue),
                new AddSupportingDocumentMenuItem(_activator, catalogue),
                new AddAggregateMenuItem(_activator, catalogue),
                new AddLookupMenuItem(_activator, "Add new Lookup Table Relationship", catalogue, null),
                new AddCatalogueItemMenuItem(_activator, catalogue)
                );

            Items.Add(addItem);

            Items.Add(new ToolStripSeparator());

            Items.Add("Clone Catalogue", null, (s, e) => CloneCatalogue(catalogue));
            /////////////////////////////////////////////////////////////Catalogue Items sub menu///////////////////////////
            Items.Add(new ToolStripSeparator());

            AddImportOptions();

            AddCommonMenuItems();
        }

        private void AddImportOptions()
        {
            //Things that are always visible regardless
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator));
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, true));
            Add(new ExecuteCommandCreateNewEmptyCatalogue(_activator));
        }


        private void CloneCatalogue(Catalogue c)
        {

            if (c != null)
            {
                if (DialogResult.Yes ==
                    MessageBox.Show("Confirm creating a duplicate of Catalogue \"" + c.Name + "\"?",
                                    "Create Duplicate?", MessageBoxButtons.YesNo))
                {

                    try
                    {
                        CatalogueCloner cloner = new CatalogueCloner(RepositoryLocator.CatalogueRepository, RepositoryLocator.CatalogueRepository);
                        cloner.CreateDuplicateInSameDatabase(c);

                    }
                    catch (Exception exception)
                    {
                        if (exception.Message.Contains("runcated"))
                            //skip the t because unsure what capitalisation it will be
                            MessageBox.Show(
                                "The name of the Catalogue to clone was too long, when we tried to put _DUPLICATE on the end it resulted in error:" +
                                exception.Message);
                        else
                            MessageBox.Show(exception.Message);
                    }
                }
            }
            else
                MessageBox.Show("Select a Catalogue first (on the left)");
        }

        public void PopupChecks(ICheckable checkable)
        {
            var popupChecksUI = new PopupChecksUI("Checking " + checkable, false);
            popupChecksUI.StartChecking(checkable);
        }
    }
}
