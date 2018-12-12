using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewFilterMatchData : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IFilter _filter;
        private readonly ViewType _viewType;
        private ColumnInfo _columnInfo;

        public ExecuteCommandViewFilterMatchData(IActivateItems activator, IFilter filter, ViewType viewType) :base(activator)
        {
            _filter = filter;
            _viewType = viewType;
            _columnInfo = filter.GetColumnInfoIfExists();

            if (_columnInfo == null)
                SetImpossible("No ColumnInfo is associated with filter '" + filter + "'");
        }
        
        public override void Execute()
        {
            base.Execute();

            Activator.ViewDataSample(new ViewColumnInfoExtractUICollection(_columnInfo, _viewType, _filter));
        }

        public override string GetCommandName()
        {
            switch (_viewType)
            {
                case ViewType.TOP_100:
                    return "View Extract";
                case ViewType.Aggregate:
                    return "View Aggregate";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Filter);
        }
    }
}