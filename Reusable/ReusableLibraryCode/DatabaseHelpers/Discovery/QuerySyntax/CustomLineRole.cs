using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    public enum CustomLineRole
    {
        None = 0,
        Axis,
        Pivot,
        TopX,
        CountFunction  //count(*) column or group by count(*) or sum(mycol) or anything that is symantically an SQL aggregate function
    }
}