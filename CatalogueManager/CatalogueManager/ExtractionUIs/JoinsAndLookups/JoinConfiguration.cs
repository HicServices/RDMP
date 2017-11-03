using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{

    /// <summary>
    /// Many researchers like flat tables they can load into SPSS or STATA or Excel and thus would prefer not to deal with multiple tables if possible.  Storing datasets as flat
    /// tables however is often suboptimal in terms of performance and storage space.  Therefore it is possible to configure a dataset (Catalogue) which includes columns from 
    /// multiple tables.  For example if you have a Header and Results table in which Header tells you when a test was done and by whom including sample volume etc and each test
    /// gives multiple results (white blood cell count, red blood cell count etc) then you will obviously want to store it as two separate tables.
    /// 
    /// Configuring join logic in RDMP enables the QueryBuilder to write an SQL SELECT query including a LEFT JOIN / RIGHT JOIN / INNER JOIN over two or more tables.  If you don't
    /// understand what an SQL Join is then you should research this before using this window.  Unlike a Lookup (See ConfigureLookups) it doesn't really matter which table contains
    /// the ForeignKey and which contains the PrimaryKey since changing the Join direction effectively inverts this logic anyway (i.e. Header RIGHT JOIN Results is the same as Results
    /// LEFT JOIN Header).  Configuring join logic in the RDMP database does not affect your data repository in any way (i.e. it doesn't mean we will suddenly start putting referential
    /// integrity constraints into your database!).
    ///  
    /// You might wonder why you have to configure JoinInfo information into RDMP when it is possibly already implemented in your data model (e.g. with foreign key constraints).  The
    /// explicit record in the RDMP database allows you to hold corrupt/unlinkable data (which would violate a foreign key constraint) and still know that the tables must be joined. 
    /// Additionally it lets you configure joins between tables in different databases and to specify an explicit direction (LEFT / RIGHT / INNER) which is always the same when it comes
    /// time to extract your data for researchers.
    /// 
    /// If you need to join on more than 1 column then just create a JoinInfo for each pair of columns (making sure the direction - LEFT/RIGHT/INNER matches).  For example if the join is
    /// Header.LabNumber = Results.LabNumber AND Header.Hospital = Results.Hospital (because of crossover in LabNumber between hospitals) then you would configure a JoinInfo for 
    /// Header.LabNumber = Results.LabNumber and another for Header.Hospital = Results.Hospital.
    /// </summary>
    public partial class JoinConfiguration : JoinConfiguration_Design
    {
        private TableInfo _leftTableInfo;
        private TableInfo _rightTableInfo;

        public JoinConfiguration()
        {
            InitializeComponent();
            fk1.KeyType = JoinKeyType.ForeignKey;
            fk2.KeyType = JoinKeyType.ForeignKey;
            fk3.KeyType = JoinKeyType.ForeignKey;

            olvLeftColumns.RowHeight = 19;
            olvRightColumns.RowHeight = 19;
        }
        
        public override void SetDatabaseObject(IActivateItems activator, TableInfo databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            olvLeftColumnNames.ImageGetter = (o) => activator.CoreIconProvider.GetImage(o);
            olvRightColumnNames.ImageGetter = (o) => activator.CoreIconProvider.GetImage(o);

            _leftTableInfo = databaseObject;
            tbLeftTableInfo.Text = _leftTableInfo.ToString();

            btnChooseRightTableInfo.Image = activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Add);
            UpdateValidityAssesment();
            
            olvLeftColumns.ClearObjects();
            olvLeftColumns.AddObjects(_leftTableInfo.ColumnInfos);
        }

        private void btnChooseRightTableInfo_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_leftTableInfo.Repository.GetAllObjects<TableInfo>("WHERE ID <>" + _leftTableInfo.ID), false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
                SetRightTableInfo((TableInfo) dialog.Selected);
        }

        private void SetRightTableInfo(TableInfo t)
        {
            //it's the same as the last one!
            if(_rightTableInfo != null && _rightTableInfo.ID == t.ID)
                return;
            
            _rightTableInfo = t;
            tbRightTableInfo.Text = t.ToString();

            fk1.Clear();
            fk2.Clear();
            fk3.Clear();

            olvRightColumns.ClearObjects();
            olvRightColumns.AddObjects(_rightTableInfo.ColumnInfos);
        }

        private void k_SelectedColumnChanged()
        {
            UpdateValidityAssesment();
        }

        private void UpdateValidityAssesment(bool actuallyDoIt = false)
        {
            ragSmiley1.Reset();
            try
            {
                var fks = new ColumnInfo[] {fk1.SelectedColumn, fk2.SelectedColumn, fk3.SelectedColumn}.Where(f => f != null).ToArray();
                var pks = new ColumnInfo[] { pk1.SelectedColumn, pk2.SelectedColumn, pk3.SelectedColumn }.Where(p => p != null).ToArray();

                if(fk1.SelectedColumn == null || pk1.SelectedColumn == null)
                    throw new Exception("You must specify at least one pair of keys to join on, do this by dragging columns out of the collection into the key boxes");

                if(
                    ((pk2.SelectedColumn == null) != (fk2.SelectedColumn == null))
                    ||
                    ((pk3.SelectedColumn == null) != (fk3.SelectedColumn == null)))
                        throw new Exception("You must have the same number of primary and foregin keys (they must come in pairs)");

                if(pks.Any(p => p.TableInfo_ID != _leftTableInfo.ID))
                    throw new Exception("All Primary Keys must come from the Left hand TableInfo");

                if(fks.Any(f => f.TableInfo_ID != _rightTableInfo.ID))
                    throw new Exception("All Foreign Keys must come from the Right hand TableInfo");

                
                ExtractionJoinType joinType;
                if(rbAllLeftHandTableRecords.Checked)
                    joinType = ExtractionJoinType.Right; //confusing I know, basically JoinInfo database record has fk,pk and direction field assuming fk joins via that direction to pk which is the opposite to the layout of this form
                else
                if(rbAllRightHandTableRecords.Checked)
                    joinType = ExtractionJoinType.Left;
                else if (rbJoinInner.Checked)
                    joinType = ExtractionJoinType.Inner;
                else
                    throw new Exception("You must select an Extraction Join direction");
                
                var cataRepo = (CatalogueRepository) _leftTableInfo.Repository;

                if (actuallyDoIt)
                {
                    for (int i = 0; i < pks.Length; i++)
                        cataRepo.JoinInfoFinder.AddJoinInfo(fks[i], pks[i], joinType, tbCollation.Text);

                    MessageBox.Show("Successfully Created Joins");
                    _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_leftTableInfo));

                    foreach (KeyDropLocationUI ui in new[] {pk1, pk2, pk3, fk1, fk2, fk3})
                        ui.Clear();
                }
                else
                    btnCreateJoinInfo.Enabled = true;
            }
            catch (Exception ex)
            {
                btnCreateJoinInfo.Enabled = false;
                ragSmiley1.Fatal(ex);
            }
        }

        private void btnCreateJoinInfo_Click(object sender, EventArgs e)
        {
            UpdateValidityAssesment(true);
        }
        
        private void tbFilterLeft_TextChanged(object sender, EventArgs e)
        {
            olvLeftColumns.UseFiltering = true;
            olvLeftColumns.ModelFilter = new TextMatchFilter(olvLeftColumns, tbFilterLeft.Text,StringComparison.CurrentCultureIgnoreCase);
        }

        private void tbFilterRight_TextChanged(object sender, EventArgs e)
        {
            olvRightColumns.UseFiltering = true;
            olvRightColumns.ModelFilter = new TextMatchFilter(olvRightColumns, tbFilterRight.Text, StringComparison.CurrentCultureIgnoreCase);

        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            UpdateValidityAssesment();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<JoinConfiguration_Design, UserControl>))]
    public abstract class JoinConfiguration_Design:RDMPSingleDatabaseObjectControl<TableInfo>
    {
    }
}
