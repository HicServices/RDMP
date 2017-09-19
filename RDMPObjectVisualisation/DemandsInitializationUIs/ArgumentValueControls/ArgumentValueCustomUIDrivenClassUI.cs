using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// This Control is for setting Properties that are of Type derrived from ICustomUIDrivenClass and require a specific plugin user interface to be displayed in order to let the user edit
    /// the value he wants (e.g. configure a web service endpoint with many properties that should be serialised / configured through a specific UI you have written).
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueCustomUIDrivenClassUI : UserControl, IArgumentValueUI
    {
        private Argument _argument;
        private ICatalogueRepository _repository;
        private DataTable _previewIfAny;

        Type _uiType;
        private DemandsInitializationAttribute _demand;

        public ArgumentValueCustomUIDrivenClassUI()
        {
            InitializeComponent();
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _argument = argument;
            _previewIfAny = previewIfAny;
            _repository = (ICatalogueRepository) _argument.Repository;
            _demand = requirement.Demand;

            ragSmiley1.Reset();

            try
            {
                Type t = _argument.GetSystemType();

                string expectedUIClassName = t.FullName + "UI";

                _uiType = _repository.MEF.GetTypeByNameFromAnyLoadedAssembly(expectedUIClassName);

                //if we did not find one with the exact name (including namespace), try getting it just by the end of it's name (omit namespace)
                if (_uiType == null)
                {
                    string shortUIClassName = t.Name + "UI";
                    var candidates = _repository.MEF.GetAllTypes().Where(type => type.Name.Equals(shortUIClassName)).ToArray();

                    if (candidates.Length > 1)
                        throw new Exception("Found " + candidates.Length + " classes called '" + shortUIClassName + "' : (" + string.Join(",", candidates.Select(c => c.Name)) + ")");

                    if (candidates.Length == 0)
                        throw new Exception("Could not find UI class called " + shortUIClassName + " make sure that it exists, is public and is marked with class attribute [Export(typeof(ICustomUI<>))]");

                    _uiType = candidates[0];
                }

    
                btnLaunchCustomUI.Text = "Launch Custom UI (" + _uiType.Name + ")";
                btnLaunchCustomUI.Width = btnLaunchCustomUI.PreferredSize.Width;
                ragSmiley1.Left = btnLaunchCustomUI.Right;

                BombIfMandatoryAndEmpty();
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
                btnLaunchCustomUI.Enabled = false;
            }
        }
        
        private void btnLaunchCustomUI_Click(object sender, System.EventArgs e)
        {
            try
            {
                var dataClassInstance = (ICustomUIDrivenClass)_argument.GetValueAsSystemType();

                var uiInstance = Activator.CreateInstance(_uiType);
                
                ICustomUI instanceAsCustomUI = (ICustomUI) uiInstance;
                instanceAsCustomUI.CatalogueRepository = _repository;

                instanceAsCustomUI.SetGenericUnderlyingObjectTo(dataClassInstance, _previewIfAny);
                var dr = ((Form)instanceAsCustomUI).ShowDialog();

                if(dr != DialogResult.Cancel)
                {
                    var result = instanceAsCustomUI.GetGenericFinalStateOfUnderlyingObject();

                    _argument.SetValue(result);
                    _argument.SaveToDatabase();
                }
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void BombIfMandatoryAndEmpty()
        {
            if (_demand.Mandatory && string.IsNullOrWhiteSpace(_argument.Value))
                ragSmiley1.Fatal(
                    new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }
    }
}
