using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCheck : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ICheckable _checkable;
        private readonly Action<ICheckable, CheckResult> _reportWorstTo;

        public ExecuteCommandCheck(IActivateItems activator, DatabaseEntity checkable): base(activator)
        {
            _checkable = checkable as ICheckable;

            if(_checkable == null)
                SetImpossible("Object is not checkable");
        }
        public ExecuteCommandCheck(IActivateItems activator, DatabaseEntity checkable,Action<ICheckable,CheckResult> reportWorst): this(activator,checkable)
        {
            _reportWorstTo = reportWorst;
        }

        public override void Execute()
        {
            base.Execute();

            var popupChecksUI = new PopupChecksUI("Checking " + _checkable, false);

            if(_reportWorstTo != null)
                popupChecksUI.AllChecksComplete += (s,a)=>_reportWorstTo(_checkable,a.CheckResults.GetWorst());

            popupChecksUI.StartChecking(_checkable);

        }


        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.TinyYellow;
        }
    }
}