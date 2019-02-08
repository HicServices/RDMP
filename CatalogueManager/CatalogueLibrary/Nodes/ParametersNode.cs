// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    public class ParametersNode:IOrderable
    {
        private readonly ISqlParameter[] _parameters;
        public ICollectSqlParameters Collector { get; set; }

        public ParametersNode(ICollectSqlParameters collector, ISqlParameter[] parameters)
        {
            _parameters = parameters;
            Collector = collector;
        }

        public override string ToString()
        {
            return _parameters.Length + " parameters (" + string.Join(",", _parameters.Select(p=>p.ParameterName)) + ")";
        }

        public override int GetHashCode()
        {
            return Collector.GetHashCode() * typeof(ParametersNode).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ParametersNode;

            if (other == null)
                return false;

            return other.GetHashCode() == GetHashCode();
        }

        public int Order { get { return -9999; } set{} }
    }
}
