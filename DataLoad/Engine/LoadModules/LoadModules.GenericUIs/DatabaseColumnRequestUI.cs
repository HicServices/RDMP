using System;
using System.Windows.Forms;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace LoadModules.GenericUIs
{
    public partial class DatabaseColumnRequestUI : UserControl
    {
        private readonly DatabaseColumnRequest _column;
        private bool bLoaded = false;

        public DatabaseColumnRequestUI(DatabaseColumnRequest column)
        {
            _column = column;
            InitializeComponent();

            lblColumnName.Text = column.ColumnName;

            ddManagedType.DataSource = DatabaseTypeRequest.PreferenceOrder;

            var request = column.TypeRequested;
            if (request != null)
            {
                if (request.CSharpType != null)
                    ddManagedType.SelectedItem = request.CSharpType;

                if (request.DecimalPlacesBeforeAndAfter != null && request.DecimalPlacesBeforeAndAfter.NumbersBeforeDecimalPlace.HasValue)
                    nBeforeDecimal.Value = request.DecimalPlacesBeforeAndAfter.NumbersBeforeDecimalPlace.Value;

                if (request.DecimalPlacesBeforeAndAfter != null && request.DecimalPlacesBeforeAndAfter.NumbersAfterDecimalPlace.HasValue)
                    nAfterDecimal.Value = request.DecimalPlacesBeforeAndAfter.NumbersAfterDecimalPlace.Value;

                if (request.MaxWidthForStrings.HasValue)
                    nLength.Value = request.MaxWidthForStrings.Value;
            }

            tbExplicitDbType.Text = column.ExplicitDbType;

            bLoaded = true;
        }

        private void tbExplicitDbType_TextChanged(object sender, System.EventArgs e)
        {

            ddManagedType.Enabled = string.IsNullOrWhiteSpace(tbExplicitDbType.Text);
            nAfterDecimal.Enabled = string.IsNullOrWhiteSpace(tbExplicitDbType.Text);
            nBeforeDecimal.Enabled = string.IsNullOrWhiteSpace(tbExplicitDbType.Text);
            nLength.Enabled = string.IsNullOrWhiteSpace(tbExplicitDbType.Text);
            
            if (!bLoaded)
                return;

            _column.ExplicitDbType = tbExplicitDbType.Text;
        }

        private void ddManagedType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!bLoaded)
                return;

            if(_column.TypeRequested != null)
                _column.TypeRequested.CSharpType = (Type) ddManagedType.SelectedItem;
        }

        private void n_ValueChanged(object sender, EventArgs e)
        {
            if (!bLoaded)
                return;

            var n = (NumericUpDown) sender;

            if (_column.TypeRequested != null)
            {
                if(n == nLength)
                    _column.TypeRequested.MaxWidthForStrings = (int)n.Value;

                if (n == nAfterDecimal && _column.TypeRequested.DecimalPlacesBeforeAndAfter != null)
                    _column.TypeRequested.DecimalPlacesBeforeAndAfter.NumbersAfterDecimalPlace = (int)n.Value;

                if (n == nBeforeDecimal && _column.TypeRequested.DecimalPlacesBeforeAndAfter != null)
                    _column.TypeRequested.DecimalPlacesBeforeAndAfter.NumbersBeforeDecimalPlace = (int)n.Value;
            }
        }
    }
}
