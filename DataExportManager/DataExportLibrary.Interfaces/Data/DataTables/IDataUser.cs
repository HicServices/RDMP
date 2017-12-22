using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See DataUser
    /// </summary>
    public interface IDataUser:IMapsDirectlyToDatabaseTable
    {
        string Forename { get; set; }
        string Surname { get; set; }
        string Email { get; set; }
    }
}