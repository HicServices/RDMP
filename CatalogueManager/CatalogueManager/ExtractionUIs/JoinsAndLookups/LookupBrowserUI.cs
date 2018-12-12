using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.Copying;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{
    public partial class LookupBrowserUI : LookupBrowserUI_Design
    {
        public LookupBrowserUI()
        {
            InitializeComponent();
        }

        private ColumnInfo _keyColumn;
        private ColumnInfo _descriptionColumn;
        private TableInfo _tableInfo;
        private Scintilla _scintilla;

        public override void SetDatabaseObject(IActivateItems activator, Lookup databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _keyColumn = databaseObject.PrimaryKey;
            _descriptionColumn = databaseObject.Description;
            _tableInfo = _keyColumn.TableInfo;

            lblCode.Text = _keyColumn.GetRuntimeName();
            lblDescription.Text = _descriptionColumn.GetRuntimeName();

            ScintillaTextEditorFactory factory = new ScintillaTextEditorFactory();
            _scintilla = factory.Create();

            gbScintilla.Controls.Add(_scintilla);

            SendQuery();
        }

        public string GetCommand()
        {
            var qb = new QueryBuilder("distinct", null);
            qb.AddColumn(new ColumnInfoToIColumn(_keyColumn){Order = 0});
            qb.AddColumn(new ColumnInfoToIColumn(_descriptionColumn){Order = 1});
            qb.TopX = 100;
            
            var container = new SpontaneouslyInventedFilterContainer(null, null, FilterContainerOperation.AND);

            if(!string.IsNullOrWhiteSpace(tbCode.Text))
            {
                var codeFilter = new SpontaneouslyInventedFilter(container, _keyColumn.GetFullyQualifiedName() + " LIKE '" + tbCode.Text + "%'", "Key Starts", "", null);
                container.AddChild(codeFilter);
            }
            
            if(!string.IsNullOrWhiteSpace(tbDescription.Text))
            {
                var codeFilter = new SpontaneouslyInventedFilter(container, _descriptionColumn.GetFullyQualifiedName() + " LIKE '%" + tbDescription.Text + "%'", "Description Contains", "", null);
                container.AddChild(codeFilter);
            }
            
            qb.RootFilterContainer = container;

            return qb.SQL;
        }

        private void tb_TextChanged(object sender, System.EventArgs e)
        {
            SendQuery();
        }

        private void SendQuery()
        {
            var tbl = _tableInfo.Discover(DataAccessContext.InternalDataProcessing);
            var server = tbl.Database.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                var sql = GetCommand();

                _scintilla.ReadOnly = false;
                _scintilla.Text = sql;
                _scintilla.ReadOnly = true;

                var da = server.GetDataAdapter(sql, con);

                var dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;


                //set autosize mode
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupBrowserUI_Design, UserControl>))]
    public abstract class LookupBrowserUI_Design: RDMPSingleDatabaseObjectControl<Lookup>
    {
    }
}
