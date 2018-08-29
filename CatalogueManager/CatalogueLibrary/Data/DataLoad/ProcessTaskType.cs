namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// The high level type of a ProcessTask, defines what the property Path contains.  If the ProcessTaskType is Executable then Path contains the path to an exe to run, If
    /// ProcessTaskType is Attacher then Path will be a class name etc.
    /// </summary>
    public enum ProcessTaskType
    {
        /// <summary>
        /// ProcessTask is to launch an executable file with parameters telling it about the load stage being operated on (servername, database name etc)
        /// </summary>
        Executable,

        /// <summary>
        /// ProcessTask is to run an SQL file directly on the server
        /// </summary>
        SQLFile,

        /// <summary>
        /// ProcessTask is to execute a stored proceedure defined on the server
        /// </summary>
        StoredProcedure,

        /// <summary>
        /// ProcessTask is to instantiate the IAttacher class Type specified in Path and hydrate it's [DemandsInitialization] properties with values matching 
        /// ProcessTaskArguments and run it in the specified load stage in an AttacherRuntimeTask wrapper.
        /// </summary>
        Attacher,

        /// <summary>
        /// ProcessTask is to instantiate the IDataProvider class Type specified in Path and hydrate it's [DemandsInitialization] properties with values matching 
        /// ProcessTaskArguments and run it in the specified load stage in an DataProviderRuntimeTask wrapper.
        /// </summary>
        DataProvider,
        
        /// <summary>
        /// ProcessTask is to instantiate the IMutilateDataTables class Type specified in Path and hydrate it's [DemandsInitialization] properties with values matching 
        /// ProcessTaskArguments and run it in the specified load stage in an MutilateDataTablesRuntimeTask wrapper.
        /// </summary>
        MutilateDataTable
    }
}