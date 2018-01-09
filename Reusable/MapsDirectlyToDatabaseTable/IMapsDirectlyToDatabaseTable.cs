using System.ComponentModel;
using System.Data.Common;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Indicates that a class cannot exist in memory without simultaneously existing as a record in a database table.  This is how RDMP handles continuous access
    /// by multiple users and persistence of objects as well as allowing for enforcing program logic via database constraints.  
    /// 
    /// RDMP basically treats the database as main memory and has many classes which are directly checked out, modified and saved into the database.  These 
    /// classes must follow strict rules e.g. all public properties must directly match columns in the database table holding them (See DatabaseEntity).  This is
    /// done in order to prevent corruption / race conditions / data loass etc in a multi user environment.
    /// </summary>
    public interface IMapsDirectlyToDatabaseTable
    {
        int ID { get; set; }
        
        [NoMappingToDatabase]
        IRepository Repository { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void SetReadOnly();

        //you must have a Property for each thing in your database table (With the same name)

        //you may have a public static field called X_MaxLength for each of these Properties

        //use MapsDirectlyToDatabaseTableRepository to fully utilise this interface

        //ensure you have a the same class name as the table name DIRECTLY
        //ensure you have a constructor that initializes your object when passed a DbDataReader (paramter value) and DbCommand (how to update yourself)
        //these two things are required for MapsDirectlyToDatabaseTable.GetAllObjects and MapsDirectlyToDatabaseTable.GetObjectByID
    }
}
