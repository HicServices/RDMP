using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are of of a known colleciton type e.g. TableInfo (from all TableInfos in a dle configuration).</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueComboBoxUI : UserControl, IArgumentValueUI
    {
        private readonly object[] _objectsForComboBox;
        private Argument _argument;
        private DemandsInitializationAttribute _demand;
        private bool _bLoading = true;

        private const string ClearSelection = "<<Clear Selection>>";

        public ArgumentValueComboBoxUI(object[] objectsForComboBox, bool useDropdownDataSourceAndOmmitClear = false)
        {
            _objectsForComboBox = objectsForComboBox;
            InitializeComponent();

            btnPick.Enabled = objectsForComboBox.All(o => o is IMapsDirectlyToDatabaseTable);

            if(!useDropdownDataSourceAndOmmitClear)
            {
                cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxValue.Items.AddRange(objectsForComboBox);
                cbxValue.Items.Add(ClearSelection);
            }
            else
            {
                cbxValue.DataSource = objectsForComboBox;
                cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _bLoading = true;
            _argument = argument;
            _demand = requirement.Demand;

            object currentValue = null;

            try
            {
                currentValue = argument.GetValueAsSystemType();
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
            }
            if (currentValue != null)
                cbxValue.Text = currentValue.ToString();

            BombIfMandatoryAndEmpty();
            _bLoading = false;
        }

        private void cbxValue_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        private void cbxValue_TextChanged(object sender, System.EventArgs e)
        {
            if (_bLoading)
                return;

            ragSmiley1.Reset();

           
            //user chose to clear selection from a combo box
            if (cbxValue.Text == ClearSelection)
                _argument.Value = null;
            else if (cbxValue.SelectedItem != null)
                _argument.SetValue(cbxValue.SelectedItem);

            _argument.SaveToDatabase();

            try
            {
                _argument.GetValueAsSystemType();
            }
            catch (Exception exception)
            {
                ragSmiley1.Fatal(exception);
            }

            BombIfMandatoryAndEmpty();
            
        }

        private void BombIfMandatoryAndEmpty()
        {
            if (_demand.Mandatory && string.IsNullOrWhiteSpace(cbxValue.Text))
                ragSmiley1.Fatal(new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }

        private void btnPick_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_objectsForComboBox.Cast<IMapsDirectlyToDatabaseTable>(), true,false);

            if (dialog.ShowDialog() == DialogResult.OK)
                if (dialog.Selected == null)
                    cbxValue.Text = ClearSelection;
                else
                    cbxValue.SelectedItem = dialog.Selected;
        }
    }
}
