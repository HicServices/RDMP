namespace CatalogueLibrary
{
    /// <summary>
    /// Final code a Data Load exits with
    /// </summary>
    public enum ExitCodeType
    {
        /// <summary>
        /// The load was succesful, there should be new/updated rows in the live database
        /// </summary>
        Success,

        /// <summary>
        /// The load failed, no new data should be in live (Due to the RAW=>STAGING=>LIVE containment system).
        /// </summary>
        Error,

        /// <summary>
        /// The load was cancelled mid way through by the user or a load component in an unexpected manner
        /// </summary>
        Abort,


        /// <summary>
        /// The load was ended mid way through by a load component which decided the load wasn't required after all (e.g. an FTP 
        /// server was empty).  This is considered to be a clean shutdown and not an error.
        /// </summary>
        OperationNotRequired
    }
}
