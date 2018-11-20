using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
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
        private bool _bLoading = true;

        private const string ClearSelection = "<<Clear Selection>>";

        private HashSet<Type> types;
        private ArgumentValueUIArgs _args;
        private bool _isEnum;

        public ArgumentValueComboBoxUI(object[] objectsForComboBox)
        {
            _objectsForComboBox = objectsForComboBox;
            InitializeComponent();

            btnPick.Enabled = objectsForComboBox.All(o => o is IMapsDirectlyToDatabaseTable);

            //If it is a dropdown of Types
            if (objectsForComboBox.All(o=>o is Type))
            {
                //add only the names (not the full namespace)
                types = new HashSet<Type>(objectsForComboBox.Cast<Type>());

                cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxValue.Items.AddRange(types.Select(t=>t.Name).ToArray());
                cbxValue.Items.Add(ClearSelection);
            }
            else
            if (objectsForComboBox.All(t=>t is Enum)) //don't offer "ClearSelection" if it is an Enum list
            {
                _isEnum = true;
                cbxValue.DataSource = objectsForComboBox;
                cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else
            {
                cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxValue.Items.AddRange(objectsForComboBox);
                cbxValue.Items.Add(ClearSelection);
            }
            
        }

        public void SetUp(ArgumentValueUIArgs args)
        {
            _bLoading = true;
            _args = args;

            object currentValue = null;

            try
            {
                if (_isEnum && _args.InitialValue == null)
                    args.Setter(cbxValue.SelectedItem);
                else
                    currentValue = _args.InitialValue;
            }
            catch (Exception e)
            {
                _args.Fatal(e);
            }

            if (currentValue != null)
                if (types != null)
                    cbxValue.Text = ((Type) currentValue).Name;
                else
                    cbxValue.Text = currentValue.ToString();

            _bLoading = false;
        }
        
        private void cbxValue_TextChanged(object sender, System.EventArgs e)
        {
            if (_bLoading)
                return;
            
            //user chose to clear selection from a combo box
            if (cbxValue.Text == ClearSelection)
                _args.Setter(null);
            else 
                if (cbxValue.SelectedItem != null)
                    if (types != null)
                        _args.Setter(types.Single(t => t.Name.Equals(cbxValue.SelectedItem)));
                    else
                        _args.Setter(cbxValue.SelectedItem);
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
