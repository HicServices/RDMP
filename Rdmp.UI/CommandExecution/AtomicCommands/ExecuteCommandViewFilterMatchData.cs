// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.UI.DataViewing;
using Rdmp.UI.DataViewing.Collections;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
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

            _candidates = catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Select(e => e.ColumnInfo).Where(c=>c != null).Distinct().ToArray();

            if (!_candidates.Any())
                SetImpossible("No ColumnInfo is associated with filter '" + filter + "'");
        }
        
        public override void Execute()
        {
            base.Execute();

            if (_columnInfo == null)
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

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Filter);
        }
    }
}