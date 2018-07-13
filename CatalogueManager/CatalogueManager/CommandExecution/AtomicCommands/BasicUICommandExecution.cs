using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;
using ScintillaNET;

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

        /// <summary>
        /// Prompts user to select 1 of the objects of type T in the list you provide
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableObjects"></param>
        /// <returns></returns>
        protected T SelectOne<T>(IList<T> availableObjects) where T : DatabaseEntity
        {
            if (availableObjects.Count == 1)
                return availableObjects[0];

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);

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
        protected bool SelectOne<T>(IList<T> availableObjects, out T selected) where T : DatabaseEntity
        {
            if (availableObjects.Count == 1)
            {
                selected = availableObjects[0];
                return true;
            }

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);
            
            selected = dialog.ShowDialog() == DialogResult.OK? (T) dialog.Selected:null;

            return selected != null;
        }

        /// <summary>
        /// Prompts user to select 1 object of type T from all the ones stored in the repository provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <returns></returns>
        protected T SelectOne<T>(IRepository repository) where T : DatabaseEntity
        {
            var availableObjects = repository.GetAllObjects<T>();
            
            if (availableObjects.Length == 1)
                return availableObjects[0];

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
                return (T)dialog.Selected;

            return null;
        }

        /// <summary>
        /// Prompts user to select 1 of the objects of type T from all the ones stored in the repository provided, returns true if they made a non null selection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableObjects"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected bool SelectOne<T>(IRepository repository, out T selected) where T : DatabaseEntity
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(repository.GetAllObjects<T>(), false, false);

            selected = dialog.ShowDialog() == DialogResult.OK ? (T)dialog.Selected : null;

            return selected != null;
        }
        protected DiscoveredTable SelectTable(bool allowDatabaseCreation,string taskDescription)
        {
            var dialog = new ServerDatabaseTableSelectorDialog(taskDescription,true,true);

            dialog.ShowDialog();

            return dialog.SelectedTable;
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
