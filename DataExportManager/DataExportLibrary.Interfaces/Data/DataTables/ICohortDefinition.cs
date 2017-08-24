namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface ICohortDefinition
    {
        int? ID { get; set; }
        string Description { get; set; }
        int Version { get; set; }
        int ProjectNumber { get; set; }
        IExternalCohortTable LocationOfCohort { get; }

        bool IsAcceptableAsNewCohort(out string matchDescription);
    }
}