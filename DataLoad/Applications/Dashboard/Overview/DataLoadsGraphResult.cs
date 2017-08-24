namespace Dashboard.Overview
{
    public class DataLoadsGraphResult
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DataLoadsGraphResultStatus Status { get; set; }
        public string LastRun { get; set; }
    }
}