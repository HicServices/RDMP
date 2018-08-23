using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.ANOUIs.ANOTableManagement;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandAnonymiseColumnInfo : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ColumnInfo _columnInfo;

        public ExecuteCommandAnonymiseColumnInfo(IActivateItems activator, ColumnInfo columnInfo):base(activator)
        {
            _columnInfo = columnInfo;
            if (columnInfo.GetRuntimeName().StartsWith(ANOTable.ANOPrefix,StringComparison.CurrentCultureIgnoreCase))
                SetImpossible("ColumnInfo is already anonymised (Starts with \"" + ANOTable.ANOPrefix + "\"");

            if (columnInfo.ANOTable_ID != null)
                SetImpossible("ColumnInfo is already anonymised");

            if(Activator.ServerDefaults.GetDefaultFor(ServerDefaults.PermissableDefaults.ANOStore) == null)
                SetImpossible("No Default ANOStore has been configured");
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<ColumnInfoToANOTableConverterUI, ColumnInfo>(_columnInfo);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ANOColumnInfo);
        }

    }
}