using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Drawing;
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
        private readonly ICoreIconProvider _coreIconProvider;
        
        public CatalogueMenu(IActivateItems activator, Catalogue catalogue):base(activator,catalogue)
        {
            _coreIconProvider = activator.CoreIconProvider;
            SetupContextMenu(catalogue);

            if (catalogue != null)
                AddCommonMenuItems();
        }
        
        private void SetupContextMenu(Catalogue catalogue)
        {
            if(catalogue == null)//no Catalogue so blank menu with import only
            {
                AddCatalogueImportOptions();
                return;
            }

            //create right click context menu
            Add(new ExecuteCommandViewCatalogueExtractionSql(_activator).SetTarget(catalogue));

            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks(catalogue));
            
            Items.Add(new ToolStripSeparator());

            var addItem = new ToolStripMenuItem("Add",null,

                new AddSupportingSqlTableMenuItem(_activator,catalogue),
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

            AddVisibilityOptions(catalogue);

            Items.Add("Delete Catalogue", null,(s,e)=> DeleteCatalogue(catalogue));

            Items.Add(new ToolStripSeparator());

            AddCatalogueImportOptions();

        }

        
        private void AddVisibilityOptions(Catalogue catalogue)
        {
            /////////////////////////////////////////////////////////////End of Catalogue Items sub menu///////////////////////////

            var visibility = new ToolStripMenuItem("Modify Visibility Flags", CatalogueIcons.CatalogueVisibility);

            if (catalogue.IsDeprecated)
                visibility.DropDownItems.Add("Un Deprecate", null, (s, e) => SetDeprecated(catalogue, false));
            else
                visibility.DropDownItems.Add("Deprecate Catalogue", null, (s, e) => SetDeprecated(catalogue, true));

            if (catalogue.IsInternalDataset)
                visibility.DropDownItems.Add("UnSet Internal Use Only", null, (s, e) => SetInternal(catalogue, false));
            else
                visibility.DropDownItems.Add("Set Internal Use Only", null, (s, e) => SetInternal(catalogue, true));

            if (catalogue.IsColdStorageDataset)
                visibility.DropDownItems.Add("Remove from Cold Storage (make Warm)", null, (s, e) => SetColdStorage(catalogue, false));
            else
                visibility.DropDownItems.Add("Set Cold Storage", null, (s, e) => SetColdStorage(catalogue, true));

            Items.Add(visibility);

        }


        public void DeleteCatalogue(Catalogue catalogue)
        {

            DialogResult result = MessageBox.Show("Are you sure you want to delete " + catalogue + " from the database?", "Delete Record", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                catalogue.DeleteInDatabase();

                Publish(catalogue);
            }
        }

        private void SetDeprecated(Catalogue c,bool value)
        {
            //make sure they really want to do it
            if(value)
                if(MessageBox.Show("Are you sure you want to DEPRECATE the catalogue " + c.Name + "?", "Deprecate Record",MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

            c.IsDeprecated = value;
            c.SaveToDatabase();

            Publish(c);
        }
        private void SetColdStorage(Catalogue c,bool value)
        {
            c.IsColdStorageDataset = value;
            c.SaveToDatabase();

            Publish(c);
        }

        private void SetInternal(Catalogue c, bool value)
        {
            c.IsInternalDataset = value;
            c.SaveToDatabase();

            Publish(c);
        }
        

        private void AddCatalogueImportOptions()
        {
            //Things that are always visible regardless
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator));
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, true));
                
            Items.Add("Create new Empty Catalogue (Not Recommended)", _coreIconProvider.GetImage(RDMPConcept.Catalogue,OverlayKind.Problem), (s, e) => NewCatalogue());
        }

        private void ConfigureValidation(Catalogue c)
        {
            _activator.ActivateConfigureValidation(this, c);
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

        public void NewCatalogue()
        {
            var c = new Catalogue(RepositoryLocator.CatalogueRepository, "New Catalogue " + Guid.NewGuid());
            Publish(c);
        }

        public void PopupChecks(ICheckable checkable)
        {
            var popupChecksUI = new PopupChecksUI("Checking " + checkable, false);
            popupChecksUI.StartChecking(checkable);
        }
    }
}
