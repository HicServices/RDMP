namespace LoadModules.Generic.LoadProgressUpdating
{
    public enum DataLoadProgressUpdateStrategy
    {
        UseMaxRequestedDay,
        ExecuteScalarSQLInRAW,
        ExecuteScalarSQLInLIVE,
        DoNothing
    }
}