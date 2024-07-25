using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.UI.SimpleDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandViewParentTree : BasicCommandExecution, IAtomicCommand
{

    private readonly IBasicActivateItems _activator;
    private readonly object _databaseObject;
    private DescendancyList _tree;

    public ExecuteCommandViewParentTree(IBasicActivateItems activator, object databaseObject) : base(activator)
    {
        _activator = activator;
        _databaseObject = databaseObject;
        //var type = databaseObject.GetType();
        //Console.WriteLine(type.Name);
    }

    private void BuildTree()
    {
        _tree = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(_databaseObject);
    }

    public override void Execute()
    {
        base.Execute();
        BuildTree();
        //todo pop a dialog
        if (_activator.IsInteractive)
        {
            //pop the dialog
            var tree = _tree.GetUsefulParents().ToList();
            tree.Add(_databaseObject);
            var dialog = new ViewParentTreeDialog(_activator, tree);
            dialog.ShowDialog();
        }
        else
        {
            //todo see if this works (and how?)
            foreach (var item in _tree.GetUsefulParents())
            {
                Console.WriteLine(item);
            }
        }
    }
}
