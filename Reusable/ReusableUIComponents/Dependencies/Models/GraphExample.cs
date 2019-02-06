// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using QuickGraph;

namespace ReusableUIComponents.Dependencies.Models
{
    /// <summary>
    /// This is our custom data graph derived from BidirectionalGraph class using custom data types.
    /// Data graph stores vertices and edges data that is used by GraphArea and end-user for a variety of operations.
    /// Data graph content handled manually by user (add/remove objects). The main idea is that you can dynamicaly
    /// remove/add objects into the GraphArea layout and then use data graph to restore original layout content.
    /// </summary>
    public class GraphExample : BidirectionalGraph<DataVertex, DataEdge> { }
}
