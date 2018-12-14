using System;
using System.Drawing;
using System.Linq;
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
        private ColumnInfo[] _candidates;

        public ExecuteCommandViewFilterMatchData(IActivateItems activator, IFilter filter, ViewType viewType = ViewType.TOP_100) :base(activator)
        {
            _filter = filter;
            _viewType = viewType;

            _columnInfo = filter.GetColumnInfoIfExists();

            //there is a single column associated with the filter?
            if(_columnInfo != null)
                return;

            //there is no single filter
            var catalogue = filter.GetCatalogue();

            if (catalogue == null)
            {
                SetImpossible("Filter has no Catalogue");
                return;
            }

            _candidates = catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Select(e => e.ColumnInfo).Distinct().ToArray();

            if (!_candidates.Any())
                SetImpossible("No ColumnInfo is associated with filter '" + filter + "'");
        }
        
        public override void Execute()
        {
            base.Execute();

            _columnInfo = SelectOne(_candidates, _columnInfo != null ? _columnInfo.Name : "");

            if (_columnInfo == null)
                return;

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