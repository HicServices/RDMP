using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;

namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{
    public partial class LookupUI : LookupUI_Design
    {
        private Lookup _lookup;

        public LookupUI()
        {
            InitializeComponent();
        }

        public override void SetDatabaseObject(IActivateItems activator, Lookup databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _lookup = databaseObject;

            pbColumnInfo1.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo);
            pbColumnInfo2.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo);
            pbTable.Image = activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo);


            tbFk.Text = databaseObject.ForeignKey.Name;
            tbPk.Text = databaseObject.PrimaryKey.Name;
            tbTable.Text = databaseObject.Description.TableInfo.Name;

            UpdateTreeViews();
        }

        private void UpdateTreeViews()
        {
            olvCompositeJoins.ClearObjects();
            olvExtractionDescriptions.ClearObjects();

            var eis = _activator.CoreChildProvider.AllExtractionInformations.Where(ei => ei.CatalogueItem.ColumnInfo_ID == _lookup.Description_ID).ToArray();
            olvExtractionDescriptions.AddObjects(eis);

            olvCompositeJoins.AddObjects(_lookup.GetSupplementalJoins().ToArray());
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void olvExtractionDescriptions_ItemActivate(object sender, EventArgs e)
        {
            var ei = olvExtractionDescriptions.SelectedObject as ExtractionInformation;

            if(ei != null)
                _activator.RequestItemEmphasis(this,new EmphasiseRequest(ei));

        }

        private void olv_KeyUp(object sender, KeyEventArgs e)
        {
            var d = ((ObjectListView)sender).SelectedObject as IDeleteable;

            if(d != null)
            {
                _activator.DeleteWithConfirmation(this, d);
                UpdateTreeViews();
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupUI_Design, UserControl>))]
    public abstract class LookupUI_Design : RDMPSingleDatabaseObjectControl<Lookup>
    {
    }
}
