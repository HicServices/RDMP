// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewFilterMatchData : BasicCommandExecution, IAtomicCommand
    {
        private readonly IFilter _filter;
        private readonly IContainer _container;

        private readonly ViewType _viewType;
        private ColumnInfo _columnInfo;
        private ColumnInfo[] _candidates;

        /// <summary>
        /// Views an extract of data from a column that matches a given <paramref name="filter"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="filter"></param>
        /// <param name="viewType"></param>
        public ExecuteCommandViewFilterMatchData(IBasicActivateItems activator, IFilter filter, ViewType viewType = ViewType.TOP_100) : this(activator, viewType)
        {
            _filter = filter;

            _columnInfo = filter.GetColumnInfoIfExists();

            //there is a single column associated with the filter?
            if (_columnInfo != null)
                return;

            // there is no single column associated with the filter so get user to pick one of them
            PopulateCandidates(filter.GetCatalogue(), filter);
        }
        public ExecuteCommandViewFilterMatchData(IBasicActivateItems activator, IContainer container, ViewType viewType = ViewType.TOP_100) : this(activator, viewType)
        {
            _container = container;

            PopulateCandidates(container.GetCatalogueIfAny(), container);
        }

        private void PopulateCandidates(Catalogue catalogue, object rootObj)
        {
            if (catalogue == null)
            {
                SetImpossible("Filter has no Catalogue");
                return;
            }

            _candidates = catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Select(e => e.ColumnInfo).Where(c => c != null).Distinct().ToArray();

            if (!_candidates.Any())
                SetImpossible("No ColumnInfo is associated with '" + rootObj + "'");
        }


        protected ExecuteCommandViewFilterMatchData(IBasicActivateItems activator, ViewType viewType) : base(activator)
        {
            _viewType = viewType;
        }

        public override void Execute()
        {
            base.Execute();

            if (_columnInfo == null)
                _columnInfo = SelectOne(_candidates, _columnInfo != null ? _columnInfo.Name : "");

            if (_columnInfo == null)
                return;

            ViewColumnExtractCollection collection = null;

            if (_filter != null)
                collection = new ViewColumnExtractCollection(_columnInfo, _viewType, _filter);
            if (_container != null)
                collection = new ViewColumnExtractCollection(_columnInfo, _viewType, _container);

            if (collection == null)
                throw new Exception("ViewFilterMatchData Command had no filter or container");

            BasicActivator.ShowData(collection);
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