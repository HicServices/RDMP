// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Repositories.Construction;
using System;
using System.IO;

namespace Rdmp.Core.CommandExecution.AtomicCommands.DataViewing
{
    public class ExecuteCommandViewData : ExecuteCommandViewDataBase, IAtomicCommand
    {
        private readonly IViewSQLAndResultsCollection _collection;
        private readonly ViewType _viewType;

        #region Constructors

        /// <summary>
        /// Provides a view of a sample of records in a column/table
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="viewType"></param>
        /// <param name="toFile"></param>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentException"></exception>
        [UseWithObjectConstructor]
        public ExecuteCommandViewData(IBasicActivateItems activator,
            [DemandsInitialization("The object (ColumnInfo, TableInfo etc) you want to view a sample of")]
            IMapsDirectlyToDatabaseTable obj,
            [DemandsInitialization("Optional. The view mode you want to see.  Options include 'TOP_100', 'Aggregate', 'Distribution' or 'All'",DefaultValue = ViewType.TOP_100)]
            ViewType viewType = ViewType.TOP_100,
            [DemandsInitialization(ToFileDescription)]
            FileInfo toFile = null) :base(activator,toFile)
        {
            _viewType = viewType;

            if (obj is TableInfo ti)
            {
                ThrowNotBasicSelectViewType();
                _collection = new ViewTableInfoExtractUICollection(ti, _viewType);
            }
            else if (obj is ColumnInfo col)
            {
                _collection = CreateCollection(col);
            }
            else if (obj is ExtractionInformation ei)
            {
                _collection = CreateCollection(ei);
            }
            else if (obj is Catalogue cata)
            {
                ThrowNotBasicSelectViewType();
                _collection = CreateCollection(cata);
            }
            else
                throw new ArgumentException($"Object '{obj}' was not an object type compatible with this command");
            
        }

        private void ThrowNotBasicSelectViewType()
        {
            if(_viewType != ViewType.TOP_100 && _viewType != ViewType.All)
            {
                throw new ArgumentException($"Only '{nameof(ViewType.TOP_100)}' or '{nameof(ViewType.All)}' can be used for this object Type");
            }
        }

        private IViewSQLAndResultsCollection CreateCollection(Catalogue cata)
        {
            return new ViewCatalogueDataCollection(cata)
            {
                TopX = _viewType == ViewType.All ? null : 100
            };
        }

        /// <summary>
        /// Fetches the <paramref name="viewType"/> of the data in <see cref="ColumnInfo"/> <paramref name="c"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="viewType"></param>
        /// <param name="c"></param>
        public ExecuteCommandViewData(IBasicActivateItems activator, ViewType viewType, ColumnInfo c) : base(activator,null)
        {
            _viewType = viewType;
            _collection = CreateCollection(c);
        }

        public ExecuteCommandViewData(IBasicActivateItems activator, ViewType viewType, ExtractionInformation ei) : base(activator,null)
        {
            _viewType = viewType;
            _collection = CreateCollection(ei);
        }

        /// <summary>
        /// Views the top 100 records of the <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="tableInfo"></param>
        public ExecuteCommandViewData(IBasicActivateItems activator, TableInfo tableInfo) : base(activator,null)
        {
            _viewType = ViewType.TOP_100;
            _collection = new ViewTableInfoExtractUICollection(tableInfo, _viewType);
        }
        #endregion

        private IViewSQLAndResultsCollection CreateCollection(ColumnInfo c)
        {
            var toReturn = new ViewColumnExtractCollection(c, _viewType);

            if (!c.IsNumerical() && _viewType == ViewType.Distribution)
                SetImpossible("Column is not numerical");

            return toReturn;
        }

        private IViewSQLAndResultsCollection CreateCollection(ExtractionInformation ei)
        {
            var toReturn = new ViewColumnExtractCollection(ei, _viewType);
            if ((!ei.ColumnInfo?.IsNumerical() ?? false) && _viewType == ViewType.Distribution)
                SetImpossible("Column is not numerical");

            return toReturn;
        }

        public override string GetCommandName()
        {
            return "View " + _viewType.ToString().Replace("_", " ");
        }
        protected override IViewSQLAndResultsCollection GetCollection()
        {
            return _collection;
        }
    }
}