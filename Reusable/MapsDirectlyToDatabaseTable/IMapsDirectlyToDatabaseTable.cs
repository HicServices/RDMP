using System.ComponentModel;
using System.Data.Common;

namespace MapsDirectlyToDatabaseTable
{
    public interface IMapsDirectlyToDatabaseTable
    {
        int ID { get; set; }
        
        [NoMappingToDatabase]
        IRepository Repository { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        //you must have a Property for each thing in your database table (With the same name)

        //you may have a public static field called X_MaxLength for each of these Properties

        //use MapsDirectlyToDatabaseTableRepository to fully utilise this interface
       
        //ensure you have a the same class name as the table name DIRECTLY
        //ensure you have a constructor that initializes your object when passed a DbDataReader (paramter value) and DbCommand (how to update yourself)
        //these two things are required for MapsDirectlyToDatabaseTable.GetAllObjects and MapsDirectlyToDatabaseTable.GetObjectByID
    }
}
