namespace LoadModules.Generic.LoadProgressUpdating
{
    /// <summary>
    /// Approaches user/system can use to determine what Date to update a LoadProgress to after a succesful data load (See UpdateProgressIfLoadsuccessful for 
    /// description of why this is non trivial).
    /// </summary>
    public enum DataLoadProgressUpdateStrategy
    {
        /// <summary>
        /// Regardless of what data actually flowed through the data load, always use the maximum requested date 
        /// </summary>
        UseMaxRequestedDay,

        /// <summary>
        /// Run a piece of SQL in the RAW environment after AdjustRAW has completed to determine what the maximum date where data was available.
        /// </summary>
        ExecuteScalarSQLInRAW,


        /// <summary>
        /// Run a piece of SQL in the LIVE environment after the data load has completed to determine what the maximum date where data was available. 
        /// </summary>
        ExecuteScalarSQLInLIVE,

        /// <summary>
        /// Do not update the DataLoadProgress at all after succesfully load.  This might result in repeatedly loading the same batch of dates over and
        /// over if you are running an IterativeScheduledDataLoadProcess
        /// </summary>
        DoNothing
    }
}