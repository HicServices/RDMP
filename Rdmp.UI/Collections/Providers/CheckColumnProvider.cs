// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.UI.Collections.Providers;

/// <summary>
/// Handles creating the Checks column in a tree list view where the value is populated for all models that are ICheckable and you have decided to
/// run the checks.
/// </summary>
public class CheckColumnProvider
{
    private readonly TreeListView _tree;
    private readonly ICoreIconProvider _iconProvider;

    public const string ChecksColumnName = "Checks";

    public CheckColumnProvider(TreeListView tree, ICoreIconProvider iconProvider)
    {
        _tree = tree;
        _iconProvider = iconProvider;
    }

    public OLVColumn CreateColumn()
    {
        var toReturn = new OLVColumn
        {
            Text = ChecksColumnName,
            ImageGetter = CheckImageGetter,
            IsEditable = false
        };

        return toReturn;
    }

    private Task checkingTask;

    public void CheckCheckables()
    {
        if (checkingTask is { IsCompleted: false })
        {
            MessageBox.Show("Checking is already happening");
            return;
        }

        //reset the dictionary
        lock (_ocheckResultsDictionaryLock)
        {
            _checkResultsDictionary = new Dictionary<ICheckable, CheckResult>();
        }

        checkingTask = new Task(() =>
        {
            //only check the items that are visible int he listview
            foreach (var checkable in GetCheckables()) //make copy to prevent synchronization issues
            {
                var notifier = new ToMemoryCheckNotifier();
                checkable.Check(notifier);

                lock (_ocheckResultsDictionaryLock)
                {
                    _checkResultsDictionary.Add(checkable, notifier.GetWorst());
                }
            }
        });

        EnsureChecksColumnVisible();
        checkingTask.ContinueWith(
            //now load images to UI
            t => _tree.RebuildColumns(), TaskScheduler.FromCurrentSynchronizationContext());

        checkingTask.Start();
    }

    public void EnsureChecksColumnVisible()
    {
        if (_tree.InvokeRequired)
        {
            _tree.Invoke(new MethodInvoker(EnsureChecksColumnVisible));
            return;
        }

        var checksCol = _tree.AllColumns.FirstOrDefault(c => string.Equals(c.Text, ChecksColumnName));

        if (checksCol is { IsVisible: false })
        {
            checksCol.IsVisible = true;
            _tree.RebuildColumns();
        }
    }

    private readonly Lock _ocheckResultsDictionaryLock = new();
    private Dictionary<ICheckable, CheckResult> _checkResultsDictionary = new();

    public void RecordWorst(ICheckable o, CheckResult result)
    {
        lock (_checkResultsDictionary)
        {
            _checkResultsDictionary[o] = result;

            if (_tree.IndexOf(o) != -1)
                _tree.RefreshObject(o);

            EnsureChecksColumnVisible();
        }
    }

    private Bitmap CheckImageGetter(object rowobject)
    {
        if (rowobject is not ICheckable checkable)
            return null;

        lock (_ocheckResultsDictionaryLock)
        {
            if (_checkResultsDictionary.TryGetValue(checkable, out var value))
                return _iconProvider.GetImage(value).ImageToBitmap();
        }

        //not been checked yet
        return null;
    }

    public IEnumerable<ICheckable> GetCheckables() => _tree.FilteredObjects.OfType<ICheckable>();
}