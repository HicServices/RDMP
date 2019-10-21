// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Emphasis;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;


namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public abstract class BasicUICommandExecution:BasicCommandExecution
    {
        protected readonly IActivateItems Activator;

        protected BasicUICommandExecution(IActivateItems activator):base(activator)
        {
            Activator = activator;
        }

        protected void Activate(DatabaseEntity o)
        {
            var cmd = new ExecuteCommandActivate(Activator, o);
            cmd.Execute();
        }

        protected FileInfo SelectSaveFile(string filter)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = filter;
            if (sfd.ShowDialog() == DialogResult.OK)
                return new FileInfo(sfd.FileName);

            return null;
        }

        protected FileInfo SelectOpenFile(string filter)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = filter;
            if (ofd.ShowDialog() == DialogResult.OK)
                return new FileInfo(ofd.FileName);

            return null;
        }

        internal void SetDefaultIfNotExists(ExternalDatabaseServer newServer, PermissableDefaults permissableDefault, bool askYesNo)
        {
            var defaults = Activator.RepositoryLocator.CatalogueRepository.GetServerDefaults();

            var current = defaults.GetDefaultFor(permissableDefault);
            
            if(current == null)
                if(!askYesNo || YesNo($"Set as the default {permissableDefault} server?", "Set as default"))
                    defaults.SetDefault(permissableDefault,newServer);
        }

        /// <summary>
        /// Prompts user to select 1 of the objects of type T in the list you provide
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableObjects"></param>
        /// <param name="initialSearchText"></param>
        /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 <paramref name="availableObjects"/></param>
        /// <returns></returns>
        protected T SelectOne<T>(IList<T> availableObjects, string initialSearchText = null, bool allowAutoSelect = false) where T : DatabaseEntity
        {
            return SelectOne(availableObjects, out T selected, initialSearchText,allowAutoSelect) ? selected : null;
        }
        
        /// <summary>
        /// Prompts user to select 1 object of type T from all the ones stored in the repository provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="initialSearchText"></param>
        /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 compatible object in the <paramref name="repository"/></param>
        /// <returns></returns>
        protected T SelectOne<T>(IRepository repository, string initialSearchText = null, bool allowAutoSelect = false) where T : DatabaseEntity
        {
            return SelectOne(repository.GetAllObjects<T>().ToList(),out T answer,initialSearchText,allowAutoSelect) ? answer: null;
        }

        /// <summary>
        /// Prompts user to select 1 of the objects of type T from all the ones stored in the repository provided, returns true if they made a non null selection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="selected"></param>
        /// <param name="initialSearchText"></param>
        /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 compatible object in the <paramref name="repository"/></param>
        /// <returns></returns>
        protected bool SelectOne<T>(IRepository repository, out T selected, string initialSearchText = null, bool allowAutoSelect = false) where T : DatabaseEntity
        {
            return SelectOne(repository.GetAllObjects<T>().ToList(),out selected,initialSearchText,allowAutoSelect);
        }

        /// <summary>
        /// Prompts user to select 1 of the objects of type T in the list you provide, returns true if they made a non null selection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableObjects"></param>
        /// <param name="selected"></param>
        /// <param name="initialSearchText"></param>
        /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 <paramref name="availableObjects"/></param>
        /// <returns></returns>
        protected bool SelectOne<T>(IList<T> availableObjects, out T selected, string initialSearchText = null, bool allowAutoSelect = false) where T : DatabaseEntity
        {
            selected = (T)Activator.SelectOne("Select One",availableObjects.ToArray(), initialSearchText, allowAutoSelect);
            return selected != null;
        }

        protected DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
        {
            return BasicActivator.SelectDatabase(allowDatabaseCreation, taskDescription);
        }

        protected DiscoveredTable SelectTable(bool allowDatabaseCreation,string taskDescription)
        {
            return BasicActivator.SelectTable(allowDatabaseCreation, taskDescription);
        }
        
        protected DiscoveredDatabase SelectDatabase(string taskDescription)
        {
            var dialog = new ServerDatabaseTableSelectorDialog(taskDescription, false, false);

            dialog.ShowDialog();
            
            if (dialog.DialogResult != DialogResult.OK)
                return null;

            return dialog.SelectedDatabase;
        }

        
        /// <summary>
        /// Runs checks on the <paramref name="checkable"/> and calls <see cref="BasicCommandExecution.SetImpossible(string)"/> if there are any failures
        /// </summary>
        /// <param name="checkable"></param>
        protected void SetImpossibleIfFailsChecks(ICheckable checkable)
        {
            try
            {
                checkable.Check(new ThrowImmediatelyCheckNotifier());
            }
            catch (Exception e)
            {

                SetImpossible(ExceptionHelper.ExceptionToListOfInnerMessages(e));
            }
        }
    }
}
