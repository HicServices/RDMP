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
        protected T SelectOne<T>(IEnumerable<T> availableObjects) where T : DatabaseEntity
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(availableObjects, false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
                return (T)dialog.Selected;

            return null;
        }

        /// <summary>
        /// Prompts user to select 1 object of type T from all the ones stored in the repository provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <returns></returns>
        protected T SelectOne<T>(IRepository repository) where T : DatabaseEntity
        {
            var all = repository.GetAllObjects<T>();
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(all, false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
                return (T)dialog.Selected;

            return null;
        }
    }
}
