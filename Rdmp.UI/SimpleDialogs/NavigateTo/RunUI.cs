// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.SimpleDialogs.NavigateTo
{
    /// <summary>
    /// Allows you to search through and run any command (<see cref="IAtomicCommand"/>) in RDMP and lets you pick which object(s) to apply it to.
    /// </summary>
    public partial class RunUI : RDMPForm,ICommandCallerArgProvider
    {
        private readonly Dictionary<string, Type> _commandsDictionary;

        private readonly CommandCaller _commandCaller;

        private readonly Dictionary<Type,Func<object>> _argsDictionary = new Dictionary<Type, Func<object>>();

        public RunUI(IActivateItems activator):base(activator)
        {
            InitializeComponent();
            
            _commandsDictionary = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
            _argsDictionary.Add(typeof(IActivateItems),()=>activator);

            _commandCaller = new CommandCaller(this,activator.RepositoryLocator);

            var commands = _commandCaller.GetSupportedCommands(activator.RepositoryLocator.CatalogueRepository.MEF);
            
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
                        _commandCaller.ExecuteCommand(type);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }


        public IEnumerable<Type> GetIgnoredCommands()
        {
            yield return typeof(ExecuteCommandPin);
            yield return typeof(ExecuteCommandUnpin);
            yield return typeof(ExecuteCommandRefreshObject);
            yield return typeof(ExecuteCommandChangeExtractability);
            yield return typeof (ExecuteCommandOpenInExplorer);
            yield return typeof (ExecuteCommandCreateNewProcessTask);
        }

        
        public object PickOne(ParameterInfo parameterInfo, Type paramType, IMapsDirectlyToDatabaseTable[] availableObjects)
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

        public DirectoryInfo PickDirectory(ParameterInfo parameterInfo, Type paramType)
        {
            var fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == DialogResult.OK)
                return new DirectoryInfo(fb.SelectedPath);
            
            return null;
        
        }

        public void OnCommandImpossible(IAtomicCommand instance)
        {
            MessageBox.Show(instance.ReasonCommandImpossible);
        }

        public void OnCommandFinished(IAtomicCommand instance)
        {
            Close();
        }

        public void OnCommandExecutionException(IAtomicCommand instance, Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAll<T>()
        {
            return Activator.CoreChildProvider.GetAllSearchables()
                .Keys.OfType<T>()
                .Cast<IMapsDirectlyToDatabaseTable>();
        }

        public object PickValueType(ParameterInfo parameterInfo, Type paramType)
        {
            var typeTextDialog = new TypeTextOrCancelDialog("Enter Value","Enter value for '" + parameterInfo.Name + "' (" + paramType.Name + ")",1000);

            if (typeTextDialog.ShowDialog() == DialogResult.OK)
                return Convert.ChangeType(typeTextDialog.ResultText, paramType);
            
            return null;
        }

        public object PickMany(ParameterInfo parameterInfo, Type arrayElementType, IMapsDirectlyToDatabaseTable[] availableObjects)
        {
            if (!availableObjects.Any())
            {
                MessageBox.Show("There are no '" + arrayElementType.Name + "' objects in your RMDP");
                return null;
            }

            SelectIMapsDirectlyToDatabaseTableDialog selectDialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);
            selectDialog.Text = parameterInfo.Name;
            selectDialog.AllowMultiSelect = true;
                                   
            
            if (selectDialog.ShowDialog() == DialogResult.OK)
            {
                var ms = selectDialog.MultiSelected.ToList();
                var toReturn = Array.CreateInstance(arrayElementType, ms.Count);

                for(int i = 0;i<ms.Count;i++)
                    toReturn.SetValue(ms[i],i);
                
                return toReturn;
            }

            return null;
        }

        public Dictionary<Type, Func<object>> GetDelegates()
        {
            return _argsDictionary;
        }
    }
}
