// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.DataLoad.Extensions;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Copying;
using CatalogueManager.Copying.Commands;
using FAnsi.Discovery;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    public class LoadDiagramColumnNode : ICommandSource, IHasLoadDiagramState
    {
        private readonly LoadDiagramTableNode _tableNode;
        private readonly IHasStageSpecificRuntimeName _column;
        private readonly LoadBubble _bubble;
        private string _expectedDataType;
        private string _discoveredDataType;
        public string ColumnName { get; private set; }

        public LoadDiagramState State { get; set; }

        public LoadDiagramColumnNode(LoadDiagramTableNode tableNode,IHasStageSpecificRuntimeName column,LoadBubble bubble)
        {
            _tableNode = tableNode;
            _column = column;
            _bubble = bubble;
            ColumnName = _column.GetRuntimeName(_bubble.ToLoadStage());
            
            var colInfo = _column as ColumnInfo;
            var preLoadDiscarded = _column as PreLoadDiscardedColumn;

            if (preLoadDiscarded != null)
                _expectedDataType = preLoadDiscarded.SqlDataType;
            else
            if (colInfo != null)
                _expectedDataType = colInfo.GetRuntimeDataType(_bubble.ToLoadStage());
            else
                throw new Exception("Expected _column to be ColumnInfo or PreLoadDiscardedColumn but it was:" + _column.GetType().Name);
        }

        public bool IsDynamicColumn
        {
            get
            {
                if (_column is PreLoadDiscardedColumn)
                    return true;

                var colInfo = (ColumnInfo)_column;

                return colInfo.IsPrimaryKey || colInfo.ANOTable_ID != null || colInfo.GetRuntimeName().StartsWith("hic_");
            }
        }

        public override string ToString()
        {
            return ColumnName;
        }

        public string GetDataType()
        {
            return State == LoadDiagramState.Different ? _discoveredDataType : _expectedDataType;
        }
        
        public ICommand GetCommand()
        {

            var querySyntaxHelper = _tableNode.TableInfo.GetQuerySyntaxHelper();

            return new SqlTextOnlyCommand(querySyntaxHelper.EnsureFullyQualified(_tableNode.DatabaseName,null, _tableNode.TableName, ColumnName));
        }

        public object GetImage(ICoreIconProvider coreIconProvider)
        {

            //if its a ColumnInfo and RAW then use the basic ColumnInfo icon
            if (_column is ColumnInfo && _bubble <= LoadBubble.Raw)
                return coreIconProvider.GetImage(RDMPConcept.ColumnInfo);

            //otherwise use the default Live/PreLoadDiscardedColumn icon
            return coreIconProvider.GetImage(_column);
        }

        public void SetState(DiscoveredColumn discoveredColumn)
        {
            _discoveredDataType = discoveredColumn.DataType.SQLType;
            State = _discoveredDataType.Equals(_expectedDataType) ? LoadDiagramState.Found : LoadDiagramState.Different;
        }

        #region equality
        protected bool Equals(LoadDiagramColumnNode other)
        {
            return _bubble == other._bubble && Equals(_tableNode, other._tableNode) && string.Equals(ColumnName, other.ColumnName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadDiagramColumnNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) _bubble;
                hashCode = (hashCode*397) ^ (_tableNode != null ? _tableNode.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ColumnName != null ? ColumnName.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}
