using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are of Array types TableInfo[], Catalogue[] etc</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueArrayUI : UserControl, IArgumentValueUI
    {
        private readonly CatalogueRepository _catalogueRepository;
        private Argument _argument;

        public ArgumentValueArrayUI(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
            InitializeComponent();
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _argument = argument;

            SetUp(argument);
        }

        private void SetUp(Argument argument)
        {
            var value = ((Array) (argument.GetValueAsSystemType()));

            if (value == null)
                tbArray.Text = "";
            else
            {
                StringBuilder sb = new StringBuilder();

                var e = value.GetEnumerator();
                while (e.MoveNext())
                {
                    sb.Append(e.Current);
                    sb.Append(",");
                }

                tbArray.Text = sb.ToString().TrimEnd(',');
            }
            
            tbArray.ReadOnly = true;
        }

        private void btnPickDatabaseEntities_Click(object sender, EventArgs e)
        {
            var type = _argument.GetConcreteSystemType();
            var elementType = type.GetElementType();

            if(elementType == null)
                throw new NotSupportedException("No array element existed for DemandsInitialization Type " + type);

            if(!_catalogueRepository.SupportsObjectType(elementType))
                throw new NotSupportedException("CatalogueRepository does not support element "+elementType+" for DemandsInitialization Type " + type);

            var objects = _catalogueRepository.GetAllObjects(elementType);
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(objects, true, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _argument.SetValue(dialog.MultiSelected == null ? null : dialog.MultiSelected.ToArray());
                _argument.SaveToDatabase();
                SetUp(_argument);
            }
        }
    }
}
