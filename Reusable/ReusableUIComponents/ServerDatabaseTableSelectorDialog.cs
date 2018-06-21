using System;
using System.Windows.Forms;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace ReusableUIComponents
{
    /// <summary>
    /// Modal dialog that prompts you to pick a database or table (<see cref="ServerDatabaseTableSelector"/>)
    /// </summary>
    public partial class ServerDatabaseTableSelectorDialog : Form
    {
        public ServerDatabaseTableSelectorDialog(string taskDescription, bool includeTable, bool tableShouldBeNovel)
        {
            InitializeComponent();

            lblTaskDescription.Text = taskDescription;
            
            if(!includeTable)
                serverDatabaseTableSelector1.HideTableComponents();

            serverDatabaseTableSelector1.TableShouldBeNovel = tableShouldBeNovel;

            serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;
        }

        void serverDatabaseTableSelector1_SelectionChanged()
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            //novel db name
            btnCreate.Enabled = db != null && !db.Exists();
        }

        public DiscoveredDatabase SelectedDatabase { get { return serverDatabaseTableSelector1.GetDiscoveredDatabase(); } }

        public DiscoveredTable SelectedTable
        {
            get
            {
                if (string.IsNullOrWhiteSpace(serverDatabaseTableSelector1.Table))
                    return null;

                return SelectedDatabase != null
                    ? SelectedDatabase.ExpectTable(serverDatabaseTableSelector1.Table)
                    : null;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            if(!db.Exists())
            {
                db.Create();
                MessageBox.Show("Database Created");
            }

        }
    }
}
