namespace DataLoadEngine.LoadProcess
{
    /// <summary>
    /// Options for controlling which sections of a Data Load are skipped/executed (e.g. for user debugging purposes you might want to stop a load after 
    /// populating RAW if you think there is a problem with the load configuration).
    /// </summary>
    public class HICLoadConfigurationFlags
    {
        public bool ArchiveData { get; set; }
        public bool DoLoadToStaging { get; set; }
        public bool DoMigrateFromStagingToLive { get; set; }

        public HICLoadConfigurationFlags()
        {
            ArchiveData = true;
            DoLoadToStaging = true;
            DoMigrateFromStagingToLive = true;
        }
    }
}