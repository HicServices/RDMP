// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.DataViewing;
using Rdmp.UI.DataViewing.Collections;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewData : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IViewSQLAndResultsCollection _collection;
        private readonly ViewType _viewType;

        /// <summary>
        /// Fetches the <paramref name="viewType"/> of the data in <see cref="ColumnInfo"/> <paramref name="c"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="viewType"></param>
        /// <param name="c"></param>
        public ExecuteCommandViewData(IActivateItems activator,ViewType viewType, ColumnInfo c) : base(activator)
        {
            _collection = new ViewColumnInfoExtractUICollection(c, viewType);
            _viewType = viewType;

            if (!c.IsNumerical() && viewType == ViewType.Distribution)
                SetImpossible("Column is not numerical");
        }

        public override string GetCommandName()
        {
            return "View " + _viewType.ToString().Replace("_"," ");
        }

        /// <summary>
        /// Views the top 100 records of the <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="tableInfo"></param>
        [ImportingConstructor]
        public ExecuteCommandViewData(IActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _viewType = ViewType.TOP_100;
            _collection = new ViewTableInfoExtractUICollection(tableInfo,_viewType);
        }

        public override void Execute()
        {
            Activator.ViewDataSample(_collection);
        }
    }
}