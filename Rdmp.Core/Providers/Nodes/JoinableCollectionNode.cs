// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
/// Collection of queries which can be joined against when building cohorts (e.g. to find all hospital admissions within 6
/// months of a prescription for drug X).  See <see cref="JoinableCohortAggregateConfiguration"/>.
/// </summary>
public class JoinableCollectionNode : Node, IOrderable
{
    public CohortIdentificationConfiguration Configuration { get; set; }
    public JoinableCohortAggregateConfiguration[] Joinables { get; set; }

    public JoinableCollectionNode(CohortIdentificationConfiguration configuration,
        JoinableCohortAggregateConfiguration[] joinables)
    {
        Configuration = configuration;
        Joinables = joinables;
    }

    public static string GetCatalogueName() => "";

    public static IMapsDirectlyToDatabaseTable Child => null;

    public static IDataAccessPoint[] GetDataAccessPoints() => null;

    public override string ToString() => "Patient Index Table(s)";

    public static string FinalRowCount() => "";
    public int? CumulativeRowCount { set; get; }


    public static string GetStateDescription() => "";

    public static string Order() => "";

    public string ElapsedTime = "";

    public static string GetCachedQueryUseCount() => "";

    public static string DescribePurpose() =>
        @"Drop Aggregates (datasets) here to create patient index tables (Tables with interesting
patient specific dates/fields which you need to use in other datasets). For example if you are
interested in studying hospitalisations for condition X and all other patient identification 
criteria are 'in the 6 months' / 'in the 12 months' post hospitalisation date per patient)";

    protected bool Equals(JoinableCollectionNode other) => Equals(Configuration, other.Configuration);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((JoinableCollectionNode) obj);
    }

    public override int GetHashCode() =>
        (Configuration != null ? Configuration.GetHashCode() : 0) * GetType().GetHashCode();

    int IOrderable.Order
    {
        get => 9999;
        set { }
    }
}