// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Compares model objects bearing in mind that anything that is compared against IOrderable MUST come in that order.  This class is a wrapper for 
    /// ModelObjectComparer that looks out for IOrderable objects passing through and enforces that order.
    /// 
    /// <para>This class is designed to modify the behaviour of TreeListView to ensure that despite the users worst efforts, the order of IOrderable nodes is always
    /// Ascending</para>
    /// </summary>
    public class OrderableComparer : IComparer
    {
        private readonly ModelObjectComparer _modelComparer;

        public OrderableComparer(OLVColumn primarySortColumn, SortOrder primarySortOrder)
        {
            _modelComparer = new ModelObjectComparer(primarySortColumn, primarySortOrder);
        }

        public int Compare(object x, object y)
        {
            //Use IOrderable.Order
            if (x is IOrderable xOrderable && y is IOrderable yOrderable)
                return xOrderable.Order - yOrderable.Order;

            //Or use INamed.Name
             if (x is INamed xNamed && y is INamed yNamed)
                return String.Compare(xNamed.Name, yNamed.Name, StringComparison.CurrentCultureIgnoreCase);

             //or use whatever
            return _modelComparer.Compare(x, y);
        }
    }
}
