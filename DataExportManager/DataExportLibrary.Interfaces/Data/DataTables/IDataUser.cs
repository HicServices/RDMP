using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface IDataUser:IMapsDirectlyToDatabaseTable
    {
        string Forename { get; set; }
        string Surname { get; set; }
        string Email { get; set; }
    }
}