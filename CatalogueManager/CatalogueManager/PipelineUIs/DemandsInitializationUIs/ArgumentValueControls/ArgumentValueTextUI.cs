using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that can be represented as textual strings (this includes parsed types like int, Regex etc).</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueTextUI : UserControl, IArgumentValueUI
    {
        private bool _bLoading = true;
        private bool _isPassword = false;
        private ArgumentValueUIArgs _args;

        public ArgumentValueTextUI(bool isPassword)
        {
            _isPassword = isPassword;
            InitializeComponent();
        }

        public void SetUp(ArgumentValueUIArgs args)
        {
            _bLoading = true;
            _args = args;

            tbText.Text = args.InitialValue == null ? "":args.InitialValue.ToString();

            if (args.Type == typeof(DirectoryInfo))
            {
                tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tbText.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            }

            if (args.Type == typeof(FileInfo))
            {
                tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tbText.AutoCompleteSource = AutoCompleteSource.FileSystem;
            }
            
            if (_isPassword)
            {
                tbText.UseSystemPasswordChar = true;
                var val = args.InitialValue;
                tbText.Text = val != null ? ((IEncryptedString)val).GetDecryptedValue() : "";
            }

            _bLoading = false;
        }

        private void tbText_TextChanged(object sender, System.EventArgs e)
        {
            if(_bLoading)
                return;

            _args.Setter(tbText.Text);
        }
    }
}
