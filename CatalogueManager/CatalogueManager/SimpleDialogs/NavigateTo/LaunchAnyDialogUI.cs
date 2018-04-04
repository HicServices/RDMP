using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.NavigateTo
{
    /// <summary>
    /// Allows you to launch any Form or Control in the current application arbitrarily.  When you select a Form or Control it will prompt you to pick appropriate objects from your Repository to fulfil
    /// it's constructor arguments and public properties.  Don't be surprised if it crashes if you select dodgy values or 'NULL' in the chooser.
    /// 
    /// </summary>
    public partial class LaunchAnyDialogUI : Form
    {
        private readonly Dictionary<Type, string> _typeDescriptionsDictionary;
        private readonly IActivateItems _activator;

        ObjectConstructor constructor = new ObjectConstructor();

        public LaunchAnyDialogUI(Dictionary<Type, string> typeDescriptionsDictionary, IActivateItems activator)
        {
            _typeDescriptionsDictionary = typeDescriptionsDictionary;
            _activator = activator;
            InitializeComponent();

            //Needed for user interface documentation creation
            if(typeDescriptionsDictionary == null || activator == null)
                return;

            comboBox1.DataSource = typeDescriptionsDictionary.Keys.Where(IsCompatible).ToArray();
            comboBox1.DisplayMember = "Name";
            comboBox1.Focus();
            comboBox1.PropertySelector = collection => collection.Cast<Type>().Select(p => p.Name);
        }

        private bool IsCompatible(Type type)
        {
            return typeof(IRDMPSingleDatabaseObjectControl).IsAssignableFrom(type) && constructor.HasBlankConstructor(type);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = comboBox1.SelectedItem as Type;

            if (selected != null)
                tbDescription.Text = _typeDescriptionsDictionary[selected].Replace("\r\n","\r\n\r\n");
            else
                tbDescription.Clear();
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    var type = (Type)comboBox1.SelectedItem;
                    
                    //Handle Constructor Arguments
                    var instance = (IRDMPSingleDatabaseObjectControl)constructor.Construct(type);

                    LaunchDialog(_activator, instance);
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }
            }
        }

        public static void LaunchDialog(IActivateItems itemActivator, IRDMPSingleDatabaseObjectControl instance)
        {
            
            //Handle Parameters
            var picked = GetUserToPickA(itemActivator,instance.GetTypeOfT());

            if (picked == null)
                return;

            //if its a Form Type
            itemActivator.ShowRDMPSingleDatabaseObjectControl(instance, (DatabaseEntity)picked);
        }

        private static object GetUserToPickA(IActivateItems itemActivator, Type type)
        {
            var dialogue = new SelectIMapsDirectlyToDatabaseTableDialog(GetAllAvailableObjects(itemActivator,type), true, true);
            
            dialogue.ShowDialog();
            return dialogue.Selected;
        }

        private static IEnumerable<IMapsDirectlyToDatabaseTable> GetAllAvailableObjects(IActivateItems itemActivator, Type parameterType)
        {
            var repositoryLocator = itemActivator.RepositoryLocator;
            if (repositoryLocator.CatalogueRepository.SupportsObjectType(parameterType))
                return repositoryLocator.CatalogueRepository.GetAllObjects(parameterType);
            else if (repositoryLocator.DataExportRepository != null && repositoryLocator.DataExportRepository.SupportsObjectType(parameterType))
                return repositoryLocator.DataExportRepository.GetAllObjects(parameterType);
            else
                throw new NotSupportedException("Neither CatalogueRepository Nor DataExportRepository admitted to hosting objects of Type " + parameterType + " which was one of the parameters on the constructor");
        }
    }
}
