using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Checks;

namespace CatalogueManager.Collections.Providers
{
    /// <summary>
    /// Handles creating the Checks column in a tree list view where the value is populated for all models that are ICheckable and you have decided to
    /// run the checks.
    /// </summary>
    public class CheckColumnProvider
    {
        private readonly TreeListView _tree;
        private readonly ICoreIconProvider _iconProvider;

        public CheckColumnProvider(TreeListView tree, ICoreIconProvider iconProvider)
        {
            _tree = tree;
            _iconProvider = iconProvider;
        }

        public OLVColumn CreateColumn()
        {
            var toReturn = new OLVColumn();
            toReturn.Text = "Checks";
            toReturn.ImageGetter = CheckImageGetter;
            toReturn.IsEditable = false;
            return toReturn;
        }
        
        private Task checkingTask;
        public void CheckCheckables()
        {
            if (checkingTask != null && !checkingTask.IsCompleted)
            {
                MessageBox.Show("Checking is already happening");
                return;
            }

            //reset the dictionary
            lock (ocheckResultsDictionaryLock)
            {
                checkResultsDictionary = new Dictionary<ICheckable, CheckResult>();
            }

            checkingTask = new Task(() =>
            {
                //only check the items that are visible int he listview
                foreach (var checkable in GetCheckables())//make copy to prevent synchronization issues
                {
                    var notifier = new ToMemoryCheckNotifier();
                    checkable.Check(notifier);

                    lock (ocheckResultsDictionaryLock)
                        checkResultsDictionary.Add(checkable, notifier.GetWorst());
                }
            });
            
            checkingTask.ContinueWith(
                //now load images to UI
                (t) => _tree.RebuildColumns(), TaskScheduler.FromCurrentSynchronizationContext());

            checkingTask.Start();
        }
        private object ocheckResultsDictionaryLock = new object();
        Dictionary<ICheckable, CheckResult> checkResultsDictionary = new Dictionary<ICheckable, CheckResult>();

        public void RecordWorst(ICheckable o, CheckResult result)
        {
            lock (checkResultsDictionary)
            {
                if (checkResultsDictionary.ContainsKey(o))
                    checkResultsDictionary.Remove(o);

                checkResultsDictionary.Add(o, result);

                if (_tree.IndexOf(o) != -1)
                    _tree.RefreshObject(o);
            }
        }

        private object CheckImageGetter(object rowobject)
        {
            var checkable = rowobject as ICheckable;
            if (checkable == null)
                return null;

            lock (ocheckResultsDictionaryLock)
            {
                if (checkResultsDictionary.ContainsKey(checkable))
                    return _iconProvider.GetImage(checkResultsDictionary[checkable]);

            }
            //not been checked yet
            return null;
        }

        public IEnumerable<ICheckable> GetCheckables()
        {
            return _tree.FilteredObjects.OfType<ICheckable>();
        }
    }
}