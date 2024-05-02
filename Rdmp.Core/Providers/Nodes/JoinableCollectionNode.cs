// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Collection of queries which can be joined against when building cohorts (e.g. to find all hospital admissions
///     within 6
///     months of a prescription for drug X).  See <see cref="JoinableCohortAggregateConfiguration" />.
/// </summary>
public class JoinableCollectionNode : Node, IOrderable
{
    public CohortIdentificationConfiguration Configuration { get; }
    public JoinableCohortAggregateConfiguration[] Joinables { get; }

    public JoinableCollectionNode(CohortIdentificationConfiguration configuration,
        JoinableCohortAggregateConfiguration[] joinables)
    {
        Configuration = configuration;
        Joinables = joinables;
    }

    public static string GetCatalogueName()
    {
        return "";
    }

    public static IMapsDirectlyToDatabaseTable Child => null;

    public static IDataAccessPoint[] GetDataAccessPoints()
    {
        return null;
    }

    public override string ToString()
    {
        return "Patient Index Table(s)";
    }

    public static string FinalRowCount()
    {
        return "";
    }

    public int? CumulativeRowCount { set; get; }


    public static string GetStateDescription()
    {
        return "";
    }

    public static string Order()
    {
        return "";
    }

    public string ElapsedTime = "";

    public static string GetCachedQueryUseCount()
    {
        return "";
    }

    public static string DescribePurpose()
    {
        return @"Drop Aggregates (datasets) here to create patient index tables (Tables with interesting
patient specific dates/fields which you need to use in other datasets). For example if you are
interested in studying hospitalisations for condition X and all other patient identification 
criteria are 'in the 6 months' / 'in the 12 months' post hospitalisation date per patient)";
    }

    protected bool Equals(JoinableCollectionNode other)
    {
        return Equals(Configuration, other.Configuration);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((JoinableCollectionNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Configuration, GetType());
    }

    int IOrderable.Order
    {
        get => 9999;
        set { }
    }
}