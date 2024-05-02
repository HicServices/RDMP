// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core;

/// <summary>
///     Compares model objects bearing in mind that anything that is compared against IOrderable MUST come in that order.
///     This class can wrap
///     another IComparer that looks out for IOrderable objects passing through and enforces that order.
///     <para>
///         This class is designed to modify the behaviour of TreeListView to ensure that despite the users worst efforts,
///         the order of IOrderable nodes is always
///         Ascending
///     </para>
/// </summary>
public class OrderableComparer : IComparer, IComparer<object>
{
    private readonly IComparer _nestedComparer;

    public OrderableComparer(IComparer nestedComparer)
    {
        _nestedComparer = nestedComparer;
    }

    /// <summary>
    ///     Decides the order to use.  This overrides the users settings when certain situations arise e.g. two
    ///     <see cref="IOrderable" /> objects are
    ///     next to each other in the tree at the same branch (in this case reordering them could be very confusing to the
    ///     user).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(object x, object y)
    {
        var xOrder = GetOrderIfAny(x);
        var yOrder = GetOrderIfAny(y);

        //Use IOrderable.Order
        if (xOrder.HasValue && yOrder.HasValue)
            return xOrder.Value - yOrder.Value;

        if (xOrder.HasValue)
            return xOrder.Value;

        // The comparison is reversed (y is orderable) so the order must be negated to.
        if (yOrder.HasValue)
            return -yOrder.Value;

        //or use whatever the model is
        return _nestedComparer?.Compare(x, y) ??
               string.Compare(x?.ToString(), y?.ToString(), StringComparison.CurrentCulture);
    }

    private static int? GetOrderIfAny(object o)
    {
        return o switch
        {
            IOrderable orderable => orderable.Order,
            ISqlParameter => -5000,
            _ => null
        };
    }
}