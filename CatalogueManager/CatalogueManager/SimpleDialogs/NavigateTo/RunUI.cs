using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.NavigateTo
{
    public partial class RunUI : Form
    {
        private readonly IActivateItems _activator;
        private Dictionary<string, Type> _commandsDictionary;

        public RunUI(IActivateItems activator)
        {
            _activator = activator;
            InitializeComponent();
            List<Exception> ex;

            var commands = activator.RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex).Where(IsSupported);

            _commandsDictionary = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
            
            foreach (var c in commands)
            {
                var name = c.Name.Replace("ExecuteCommand", "");
                
                if(!_commandsDictionary.ContainsKey(name))
                    _commandsDictionary.Add(c.Name.Replace("ExecuteCommand", ""), c);
            }

            comboBox1.Items.AddRange(_commandsDictionary.Keys.ToArray());

        }

        private bool IsSupported(Type t)
        {

            bool acceptableType = typeof (IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;

            if (!acceptableType)
                return false;

            try
            {
                var constructors =  t.GetConstructors();

                if (constructors.Length == 0)
                    return false;

                return IsSupported(constructors[0]);

            }
            catch (Exception)
            {
                return false;
            }
        }

        private void comboBox1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            var key = (string)comboBox1.SelectedItem;

            if (key == null)
                return;
            
            if (e.KeyCode == Keys.Enter)
            {
                if (_commandsDictionary.ContainsKey(key))
                {
                    var type = _commandsDictionary[key];
                    var constructors = type.GetConstructors();

                    CallConstructor(constructors[0]);
                }
            }
        }

        private void CallConstructor(ConstructorInfo constructorInfo)
        {

            List<object> parameterValues = new List<object>();

            foreach (var parameterInfo in constructorInfo.GetParameters())
            {
                var paramType = parameterInfo.ParameterType;

                var value = GetValueForParameterOfType(parameterInfo,paramType);
                
                if(value == null)
                    throw new Exception("Could not figure out a value for property '" + parameterInfo + "' for constructor '" + constructorInfo + "'.  Parameter Type was '" + paramType + "'");

                parameterValues.Add(value);
            }

            var instance = constructorInfo.Invoke(parameterValues.ToArray());
            
            ((IAtomicCommand)instance).Execute();
        }

        public static bool IsSupported(ConstructorInfo c)
        {
            return c.GetParameters().All(
                p =>
                    typeof (ICatalogueRepository).IsAssignableFrom(p.ParameterType) ||
                    typeof (IDataExportRepository).IsAssignableFrom(p.ParameterType) ||
                    typeof (IRDMPPlatformRepositoryServiceLocator).IsAssignableFrom(p.ParameterType) ||
                    typeof (IActivateItems).IsAssignableFrom(p.ParameterType) ||
                    typeof(DirectoryInfo).IsAssignableFrom(p.ParameterType) ||
                    typeof(DatabaseEntity).IsAssignableFrom(p.ParameterType) ||
                    p.HasDefaultValue ||
                    p.ParameterType.IsValueType
                );
        }

        private object GetValueForParameterOfType(ParameterInfo parameterInfo, Type paramType)
        {
            if (typeof(ICatalogueRepository).IsAssignableFrom(paramType))
                return _activator.RepositoryLocator.CatalogueRepository;

            if (typeof(IDataExportRepository).IsAssignableFrom(paramType))
                return _activator.RepositoryLocator.DataExportRepository;

            if (typeof(IRDMPPlatformRepositoryServiceLocator).IsAssignableFrom(paramType))
                return _activator.RepositoryLocator;

            if (typeof(IActivateItems).IsAssignableFrom(paramType))
                return _activator;

            if (typeof (DirectoryInfo).IsAssignableFrom(paramType))
            {
                var fb = new FolderBrowserDialog();
                if (fb.ShowDialog() == DialogResult.OK)
                    return new DirectoryInfo(fb.SelectedPath);
                
                return null;
            }

            if (typeof(DatabaseEntity).IsAssignableFrom(paramType))
            {
                IEnumerable<IMapsDirectlyToDatabaseTable> availableObjects;
                if (_activator.RepositoryLocator.CatalogueRepository.SupportsObjectType(paramType))
                    availableObjects = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects(paramType);
                else if (_activator.RepositoryLocator.DataExportRepository.SupportsObjectType(paramType))
                    availableObjects = _activator.RepositoryLocator.DataExportRepository.GetAllObjects(paramType);
                else
                    return null;

                SelectIMapsDirectlyToDatabaseTableDialog selectDialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);

                if (selectDialog.ShowDialog() == DialogResult.OK)
                    return selectDialog.Selected;

                return null; //user didn't select one of the IMapsDirectlyToDatabaseTable objects shown in the dialog
            }

            if (parameterInfo.HasDefaultValue)
                return parameterInfo.DefaultValue;

            if (paramType.IsValueType)
            {
                var typeTextDialog = new TypeTextOrCancelDialog("Enter value for '" + parameterInfo.Name + "' (" + paramType + ")","Value",1000);

                if (typeTextDialog.ShowDialog() == DialogResult.OK)
                    return Convert.ChangeType(typeTextDialog.ResultText, paramType);
            }

            return null;
        }
    }
}
