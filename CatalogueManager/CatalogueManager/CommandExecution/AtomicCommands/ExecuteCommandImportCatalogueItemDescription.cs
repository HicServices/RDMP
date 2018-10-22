using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Allows you to copy descriptive metadata (CatalogueItems) between datasets.  This is useful for maintaining a 'single version of the truth' e.g. if every dataset has a field called 
    /// 'NHS Number' then the description of this column should be the same in every case.  Using this form you can import/copy the description from another column.  While this covers you
    /// for setting up new fields, the synchronizing of this description over time (e.g. when a data analyst edits one of the other 'NHS Number' fields) is done through propagation
    /// (See PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItems)
    /// </summary>
    internal class ExecuteCommandImportCatalogueItemDescription : BasicUICommandExecution, IAtomicCommand
    {
        private readonly CatalogueItem _toPopulate;
        
        public ExecuteCommandImportCatalogueItemDescription(IActivateItems activator, CatalogueItem toPopulate):base(activator)
        {
            _toPopulate = toPopulate;
        }

        public override void Execute()
        {
            var available = Activator.CoreChildProvider.AllCatalogueItems.Except(new[] {_toPopulate}).ToArray();
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(available, false, false);
            
            //if we have a CatalogueItem other than us that has same Name maybe that's the one they want
            if(available.Any(a=>a.Name.Equals(_toPopulate.Name,StringComparison.CurrentCultureIgnoreCase)))
                dialog.SetInitialFilter(_toPopulate.Name);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var chosen = (CatalogueItem) dialog.Selected;
                CopyNonIDValuesAcross(chosen, _toPopulate, true);
                _toPopulate.SaveToDatabase();
                
                Publish(_toPopulate);
            }
            
            base.Execute();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        /// <summary>
        /// Copies all properties (Description etc) from one CatalogueItem into another (except ID properties).
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="skipNameProperty"></param>
        public void CopyNonIDValuesAcross(IMapsDirectlyToDatabaseTable from, IMapsDirectlyToDatabaseTable to, bool skipNameProperty = false)
        {
            var type = from.GetType();

            if (to.GetType() != type)
                throw new Exception("From and To objects must be of the same Type");

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.Name == "ID")
                    continue;

                if (propertyInfo.Name.EndsWith("_ID"))
                    continue;

                if (propertyInfo.Name == "Name" && skipNameProperty)
                    continue;

                if (propertyInfo.CanWrite == false || propertyInfo.CanRead == false)
                    continue;

                object value = propertyInfo.GetValue(from, null);
                propertyInfo.SetValue(to, value, null);
            }

            var s = to as ISaveable;
            if (s != null)
                s.SaveToDatabase();
        }

    }
}