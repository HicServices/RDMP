using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public abstract class BasicUICommandExecution:BasicCommandExecution
    {
        protected readonly IActivateItems Activator;

        protected BasicUICommandExecution(IActivateItems activator)
        {
            Activator = activator;
        }

        protected void Publish(DatabaseEntity o)
        {
            Activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(o));
        }

        protected void Activate(DatabaseEntity o)
        {
            var cmd = new ExecuteCommandActivate(Activator, o);
            cmd.Execute();
        }

        protected void Emphasise(DatabaseEntity o, int expansionDepth = 0)
        {
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(o, expansionDepth));
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
        /// <summary>
        /// Prompts user to select 1 of the objects of type T in the list you provide
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableObjects"></param>
        /// <returns></returns>
        protected T SelectOne<T>(IList<T> availableObjects, string initialSearchText = null) where T : DatabaseEntity
        {
            if (availableObjects.Count == 1)
                return availableObjects[0];

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);
            dialog.SetInitialFilter(initialSearchText);

            if (dialog.ShowDialog() == DialogResult.OK)
                return (T)dialog.Selected;

            return null;
        }

        /// <summary>
        /// Prompts user to select 1 of the objects of type T in the list you provide, returns true if they made a non null selection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableObjects"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected bool SelectOne<T>(IList<T> availableObjects, out T selected, string initialSearchText = null) where T : DatabaseEntity
        {
            if (availableObjects.Count == 1)
            {
                selected = availableObjects[0];
                return true;
            }

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);
            dialog.SetInitialFilter(initialSearchText);

            selected = dialog.ShowDialog() == DialogResult.OK? (T) dialog.Selected:null;

            return selected != null;
        }

        /// <summary>
        /// Prompts user to select 1 object of type T from all the ones stored in the repository provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="initialSearchText"></param>
        /// <returns></returns>
        protected T SelectOne<T>(IRepository repository, string initialSearchText = null) where T : DatabaseEntity
        {
            var availableObjects = repository.GetAllObjects<T>();
            
            if (availableObjects.Length == 1)
                return availableObjects[0];

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);
            dialog.SetInitialFilter(initialSearchText);

            if (dialog.ShowDialog() == DialogResult.OK)
                return (T)dialog.Selected;

            return null;
        }

        /// <summary>
        /// Prompts user to select 1 of the objects of type T from all the ones stored in the repository provided, returns true if they made a non null selection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected bool SelectOne<T>(IRepository repository, out T selected, string initialSearchText = null) where T : DatabaseEntity
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(repository.GetAllObjects<T>(), false, false);
            dialog.SetInitialFilter(initialSearchText);

            selected = dialog.ShowDialog() == DialogResult.OK ? (T)dialog.Selected : null;
            
            return selected != null;
        }
        protected DiscoveredTable SelectTable(bool allowDatabaseCreation,string taskDescription)
        {
            var dialog = new ServerDatabaseTableSelectorDialog(taskDescription,true,true);

            dialog.ShowDialog();

            if (dialog.DialogResult != DialogResult.OK)
                return null;

            return dialog.SelectedTable;
        }

        protected bool SelectMany<T>(T[] available, out T[] selected, string initialSearchText = null) where T : DatabaseEntity
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(available, false, false);
            dialog.AllowMultiSelect = true;
            dialog.SetInitialFilter(initialSearchText);
            dialog.ShowDialog();
            
            if (dialog.DialogResult != DialogResult.OK)
            {
                selected = null;
                return false;
            }

            selected = dialog.MultiSelected.Cast<T>().ToArray();
            return true;
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
        /// Prompts the user to type in some text (up to a maximum length).  Returns true if they supplied some text or false if they didn't or it was blank/cancelled etc
        /// </summary>
        /// <param name="header"></param>
        /// <param name="prompt"></param>
        /// <param name="maxLength"></param>
        /// <param name="initialText"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        protected bool TypeText(string header, string prompt, int maxLength, string initialText, out string text)
        {
            var textTyper = new TypeTextOrCancelDialog(header,prompt, maxLength, initialText);
            text = textTyper.ShowDialog() == DialogResult.OK ? textTyper.ResultText : null;
            return !string.IsNullOrWhiteSpace(text);
        }

        /// <inheritdoc cref="TypeText(string, string, int, string, out string)"/>
        protected bool TypeText(string header, string prompt, out string text)
        {
            return TypeText(header, prompt, 500, null, out text);
        }
    }
}
