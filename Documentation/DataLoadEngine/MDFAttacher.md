# MDF Attacher

The MDF attacher is used for loading a detached database file into RAW.  
This attacher does not load RAW tables normally (like AnySeparatorFileAttacher etc) instead it specifies that it is itself going to act as RAW.
Using this component requires that the computer running the data load has file system access to the RAW SQL Server data directory (and that the MDF and LDF files exist in the same directory).

## Attach Strategies
The MDF Attacher offers two attach strategies
### Attach with Connection String
AttachWithConnectionString attempts to do the attaching as part of connection by specifying the AttachDBFilename keyword in the connection string
### Execute Create  Database For Attach SQL
 ExecuteCreateDatabaseForAttachSql attempts to connect to 'master' and execute CREATE DATABASE SQL with the FILENAME property set to your MDF file in the DATA directory of your database server
## Attaching an MDF from another File system type
You might want to load an MDF file from a Linux system into a windows installation of RDMP.
You will run into issues with the MDF file looking for a location like '/var/opt/mssql/data/my_db.mdf' while the file is on your file system at 'C:\Users\me\my_db.mdf'.
In order to be able to use this MDF, you will need to
* Use the 'ExecuteCreateDatabaseForAttachSql' attach strategy
* Specify an 'Override Attach MDF Path' - this can be an absolute path to your MDF file on your file system, or to the directory, assuming the MDF filename has not changed
* Specify an 'Override Attach LDF Path' - this can be an absolute path to your LDF file on your file system, or to the directory, assuming the LDF filename has not changed

If using the directory based overrides, it is assumed that both the MDF and LDF files are in the same directory.