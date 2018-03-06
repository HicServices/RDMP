namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    public enum QueryComponent
    {
        None = 0,
        VariableDeclaration,
        SELECT,
        SET,
        QueryTimeColumn,
        FROM, //do not use for CustomLines - use only for WhatIsOnLine etc
        JoinInfoJoin,
        WHERE, //relates to sql to be treated as a final bit of WHERE sql located after any containers.  Expect to have either AND or WHERE automatically injected into your start by QueryBuilders
        GroupBy,
        Having,
        OrderBy,
        Postfix //after everything else in the query (including WHERE containers and any ORDER BYs)

    }
}