using System;
using System.Windows.Forms;
using FAnsi;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;

namespace ReusableUIComponents
{
    public partial class DatabaseTypeUI : UserControl
    {
        private readonly DatabaseTypeIconProvider _databaseIconProvider;
        private DatabaseType _databaseType;
        public event EventHandler DatabaseTypeChanged;

        public DatabaseType DatabaseType
        {
            get { return _databaseType; }
            set
            {
                _databaseType = value;

                if(bLoading)
                    return;

                ddDatabaseType.SelectedItem = value;
                pbDatabaseProvider.Image = _databaseIconProvider.GetImage(value);
                
            }
        }

        private bool bLoading = true;
        public DatabaseTypeUI()
        {
            InitializeComponent();
            ddDatabaseType.DataSource = Enum.GetValues(typeof(DatabaseType));

            _databaseIconProvider = new DatabaseTypeIconProvider();
            pbDatabaseProvider.Image = _databaseIconProvider.GetImage(DatabaseType.MicrosoftSQLServer);

            bLoading = false;

        }

        public void LockDatabaseType(DatabaseType databaseType)
        {
            ddDatabaseType.SelectedItem = databaseType;
            ddDatabaseType.Enabled = false;
        }

        private bool changing = false;
        private void ddDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(changing )
                return;

            changing = true;

            DatabaseType = (DatabaseType)ddDatabaseType.SelectedItem;

            if(DatabaseTypeChanged != null)
                DatabaseTypeChanged(this,new EventArgs());

            changing = false;
        }
    }
}
