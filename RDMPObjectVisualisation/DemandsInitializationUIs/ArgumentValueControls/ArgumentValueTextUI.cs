using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// This Control is for setting Properties that can be represented as textual strings (this includes parsed types like int, Regex etc).
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueTextUI : UserControl, IArgumentValueUI
    {
        private Argument _argument;
        private DemandsInitializationAttribute _demand;
        private bool _bLoading = true;

        private bool _isPassword = false;

        public ArgumentValueTextUI(bool isPassword)
        {
            _isPassword = isPassword;
            InitializeComponent();
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _bLoading = true;
            _argument = argument;
            _demand = requirement.Demand;
            tbText.Text = argument.Value;

            if(argument.GetSystemType() == typeof(DirectoryInfo))
            {
                tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tbText.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            }

            if (argument.GetSystemType() == typeof(FileInfo))
            {
                tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tbText.AutoCompleteSource = AutoCompleteSource.FileSystem;
            }
            
            if (_isPassword)
            {
                tbText.UseSystemPasswordChar = true;
                var val = _argument.GetValueAsSystemType();
                tbText.Text = val != null ? ((IEncryptedString)val).GetDecryptedValue() : "";
            }

            BombIfMandatoryAndEmpty();
            _bLoading = false;
        }

        private void tbText_TextChanged(object sender, System.EventArgs e)
        {
            if(_bLoading)
                return;

            ragSmiley1.Reset();
            
            _argument.SetValue(tbText.Text);
            
            _argument.SaveToDatabase();

            try
            {
                _argument.GetValueAsSystemType();
                
                BombIfMandatoryAndEmpty();
            }
            catch (Exception exception)
            {
                ragSmiley1.Fatal(exception);
            }

        }

        private void BombIfMandatoryAndEmpty()
        {
            if (_demand.Mandatory && string.IsNullOrWhiteSpace(tbText.Text))
                ragSmiley1.Fatal(new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }
    }
}
