// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.SimpleDialogs.NavigateTo
{
    /// <summary>
    /// Allows you to search through and run any command (<see cref="IAtomicCommand"/>) in RDMP and lets you pick which object(s) to apply it to.
    /// </summary>
    public partial class RunUI : RDMPForm
    {
        private readonly Dictionary<string, Type> _commandsDictionary;

        public RunUI(IActivateItems activator):base(activator)
        {
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
                    typeof(ICheckable).IsAssignableFrom(p.ParameterType) ||
                    typeof(IMightBeDeprecated).IsAssignableFrom(p.ParameterType)||
                    p.HasDefaultValue ||
                    (p.ParameterType.IsValueType && !typeof(Enum).IsAssignableFrom(p.ParameterType))
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
                return Activator.RepositoryLocator.CatalogueRepository;

            if (typeof(IDataExportRepository).IsAssignableFrom(paramType))
                return Activator.RepositoryLocator.DataExportRepository;

            if (typeof(IRDMPPlatformRepositoryServiceLocator).IsAssignableFrom(paramType))
                return Activator.RepositoryLocator;

            if (typeof(IActivateItems).IsAssignableFrom(paramType))
                return Activator;

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
                if (Activator.RepositoryLocator.CatalogueRepository.SupportsObjectType(paramType))
                    availableObjects = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects(paramType).ToArray();
                else if (Activator.RepositoryLocator.DataExportRepository.SupportsObjectType(paramType))
                    availableObjects = Activator.RepositoryLocator.DataExportRepository.GetAllObjects(paramType).ToArray();
                else
                    return null;


                return PickOne(parameterInfo,paramType,availableObjects);
            }

            if (typeof (IMightBeDeprecated).IsAssignableFrom(paramType))
                return PickOne(parameterInfo,paramType,
                        Activator.CoreChildProvider.GetAllSearchables()
                        .Keys.OfType<IMightBeDeprecated>()
                        .Cast<IMapsDirectlyToDatabaseTable>()
                        .ToArray());

            if (typeof(ICheckable).IsAssignableFrom(paramType))
            {
                return PickOne(parameterInfo, paramType, Activator.CoreChildProvider.GetAllSearchables()
                    .Keys.OfType<ICheckable>()
                    .Cast<IMapsDirectlyToDatabaseTable>()
                    .Where(paramType.IsInstanceOfType)
                    .ToArray());
            }

            if (parameterInfo.HasDefaultValue)
                return parameterInfo.DefaultValue;

            if (paramType.IsValueType && !typeof(Enum).IsAssignableFrom(paramType))
            {
                var typeTextDialog = new TypeTextOrCancelDialog("Enter Value","Enter value for '" + parameterInfo.Name + "' (" + paramType.Name + ")",1000);

                if (typeTextDialog.ShowDialog() == DialogResult.OK)
                    return Convert.ChangeType(typeTextDialog.ResultText, paramType);
            }

            return null;
        }

        private object PickOne(ParameterInfo parameterInfo, Type paramType, IMapsDirectlyToDatabaseTable[] availableObjects)
        {
            if (!availableObjects.Any())
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
    }
}
