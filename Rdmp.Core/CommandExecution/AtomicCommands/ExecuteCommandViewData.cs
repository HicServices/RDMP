// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewData : BasicCommandExecution, IAtomicCommand
    {
        private readonly IViewSQLAndResultsCollection _collection;
        private readonly ViewType _viewType;

        #region Constructors
        /// <summary>
        /// Fetches the <paramref name="viewType"/> of the data in <see cref="ColumnInfo"/> <paramref name="c"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="viewType"></param>
        /// <param name="c"></param>
        public ExecuteCommandViewData(IBasicActivateItems activator, ViewType viewType, ColumnInfo c) : base(activator)
        {
            _collection = new ViewColumnExtractCollection(c, viewType);
            _viewType = viewType;

            if (!c.IsNumerical() && viewType == ViewType.Distribution)
                SetImpossible("Column is not numerical");
        }


        public ExecuteCommandViewData(IBasicActivateItems activator, ViewType viewType, ExtractionInformation ei) : base(activator)
        {
            _collection = new ViewColumnExtractCollection(ei, viewType);
            _viewType = viewType;

            if ((!ei.ColumnInfo?.IsNumerical()??false) && viewType == ViewType.Distribution)
                SetImpossible("Column is not numerical");
        }

        /// <summary>
        /// Views the top 100 records of the <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="tableInfo"></param>
        [UseWithObjectConstructor]
        public ExecuteCommandViewData(IBasicActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _viewType = ViewType.TOP_100;
            _collection = new ViewTableInfoExtractUICollection(tableInfo, _viewType);
        }
        #endregion

        public override string GetCommandName()
        {
            return "View " + _viewType.ToString().Replace("_", " ");
        }


        public override void Execute()
        {
            BasicActivator.ShowData(_collection);
        }
    }
}