using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
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
        private readonly Dictionary<string, Type> _commandsDictionary;

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
                    try
                    {
                        CallConstructor(GetConstructor(type));
                    }
                    catch (OperationCanceledException)
                    {
                    }
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
                
                //if it's a null and not a default null
                if(value == null && !parameterInfo.HasDefaultValue)
                    throw new OperationCanceledException("Could not figure out a value for property '" + parameterInfo + "' for constructor '" + constructorInfo + "'.  Parameter Type was '" + paramType + "'");

                parameterValues.Add(value);
            }

            var instance = (IAtomicCommand)constructorInfo.Invoke(parameterValues.ToArray());
            try
            {
                if (instance.IsImpossible)
                {
                    MessageBox.Show(instance.ReasonCommandImpossible);
                    return;
                }

                instance.Execute();
                Close();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
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

        public static bool IsSupported(Type t)
        {
            bool acceptableType = typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;

            if (!acceptableType)
                return false;

            if (GetIgnoredCommands().Contains(t))
                return false;

            try
            {
                var constructor = GetConstructor(t);

                if (constructor == null)
                    return false;

                return IsSupported(constructor);

            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors();

            if (constructors.Length == 0)
                return null;

            var importDecorated = constructors.Where(c => Attribute.IsDefined(c, typeof(ImportingConstructorAttribute))).ToArray();

            if (importDecorated.Any())
                return importDecorated[0];

            return constructors[0];
        }

        public static IEnumerable<Type> GetIgnoredCommands()
        {
            yield return typeof(ExecuteCommandPin);
            yield return typeof(ExecuteCommandUnpin);
            yield return typeof(ExecuteCommandRefreshObject);
            yield return typeof(ExecuteCommandChangeExtractability);
            yield return typeof (ExecuteCommandOpenInExplorer);
            yield return typeof (ExecuteCommandCreateNewProcessTask);
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
                IMapsDirectlyToDatabaseTable[] availableObjects;
                if (_activator.RepositoryLocator.CatalogueRepository.SupportsObjectType(paramType))
                    availableObjects = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects(paramType).ToArray();
                else if (_activator.RepositoryLocator.DataExportRepository.SupportsObjectType(paramType))
                    availableObjects = _activator.RepositoryLocator.DataExportRepository.GetAllObjects(paramType).ToArray();
                else
                    return null;

                if(!availableObjects.Any())
                {
                    MessageBox.Show("There are no '" + paramType.Name + "' objects in your RMDP");
                    return null;
                }
                
                SelectIMapsDirectlyToDatabaseTableDialog selectDialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);
                selectDialog.Text = parameterInfo.Name;

                if (selectDialog.ShowDialog() == DialogResult.OK)
                    return selectDialog.Selected;

                return null; //user didn't select one of the IMapsDirectlyToDatabaseTable objects shown in the dialog
            }

            if (parameterInfo.HasDefaultValue)
                return parameterInfo.DefaultValue;

            if (paramType.IsValueType)
            {
                var typeTextDialog = new TypeTextOrCancelDialog("Enter Value","Enter value for '" + parameterInfo.Name + "' (" + paramType.Name + ")",1000);

                if (typeTextDialog.ShowDialog() == DialogResult.OK)
                    return Convert.ChangeType(typeTextDialog.ResultText, paramType);
            }

            return null;
        }
    }
}
