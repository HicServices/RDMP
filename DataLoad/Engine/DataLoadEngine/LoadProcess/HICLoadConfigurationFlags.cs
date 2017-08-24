namespace DataLoadEngine.LoadProcess
{
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