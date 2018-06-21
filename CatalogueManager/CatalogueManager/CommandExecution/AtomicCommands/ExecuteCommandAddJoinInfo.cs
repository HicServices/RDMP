using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddJoinInfo : BasicUICommandExecution, IAtomicCommand
    {
        private readonly TableInfo _tableInfo;
        private TableInfo _otherTableInfo;

        public ExecuteCommandAddJoinInfo(IActivateItems activator, TableInfo tableInfo):base(activator)
        {
            _tableInfo = tableInfo;
        }

        public override string GetCommandName()
        {
            return "Configure JoinInfo where '" + _tableInfo + "' is a Primary Key Table";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return Activator.CoreIconProvider.GetImage(RDMPConcept.JoinInfo, OverlayKind.Add);
        }

        public void SetInitialJoinToTableInfo(TableInfo otherTableInfo)
        {
            if(_tableInfo.Equals(otherTableInfo))
                SetImpossible("Cannot join a TableInfo to itself");
            
            _otherTableInfo = otherTableInfo;
        }

        public override void Execute()
        {
            base.Execute();

            var jc = Activator.Activate<JoinConfiguration, TableInfo>(_tableInfo);

            if (_otherTableInfo != null)
                jc.SetOtherTableInfo(_otherTableInfo);
        }
    }
}