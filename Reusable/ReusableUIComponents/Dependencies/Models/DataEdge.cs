// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using GraphX.PCL.Common.Models;

namespace ReusableUIComponents.Dependencies.Models
{
    /* DataEdge is the data class for the edges. It contains all custom edge data specified by the user.
     * This class also must be derived from EdgeBase class that provides properties and methods mandatory for
     * correct GraphX operations.
     * Some of the useful EdgeBase members are:
     *  - ID property that stores unique positive identfication number. Property must be filled by user.
     *  - IsSelfLoop boolean property that indicates if this edge is self looped (eg have identical Target and Source vertices) 
     *  - RoutingPoints collection of points used to create edge routing path. If Null then straight line will be used to draw edge.
     *      In most cases it is handled automatically by GraphX.
     *  - Source property that holds edge source vertex.
     *  - Target property that holds edge target vertex.
     *  - Weight property that holds optional edge weight value that can be used in some layout algorithms.
     */

    public class DataEdge : EdgeBase<DataVertex>
    {
        /// <summary>
        /// Default constructor. We need to set at least Source and Target properties of the edge.
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="target">Target vertex data</param>
        /// <param name="weight">Optional edge weight</param>
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
			: base(source, target, weight)
		{
		}
        /// <summary>
        /// Default parameterless constructor (for serialization compatibility)
        /// </summary>
        public DataEdge()
            : base(null, null, 1)
        {
        }

        /// <summary>
        /// Custom string property for example
        /// </summary>
        public string Text { get; set; }

        #region GET members
        public override string ToString()
        {
            return Text;
        }
        #endregion
    }
}
