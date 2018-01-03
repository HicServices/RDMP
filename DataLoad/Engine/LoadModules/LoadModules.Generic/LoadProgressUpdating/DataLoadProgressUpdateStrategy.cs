namespace LoadModules.Generic.LoadProgressUpdating
{
    /// <summary>
    /// Approaches user/system can use to determine what Date to update a LoadProgress to after a succesful data load (See UpdateProgressIfLoadsuccessful for 
    /// description of why this is non trivial).
    /// </summary>
    public enum DataLoadProgressUpdateStrategy
    {
        UseMaxRequestedDay,
        ExecuteScalarSQLInRAW,
        ExecuteScalarSQLInLIVE,
        DoNothing
    }
}