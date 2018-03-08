using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using ADOX;


namespace DataExportLibrary.ExtractionTime.FileOutputFormats
{
    /// <summary>
    /// Helper class for outputting DataTables to Microsoft Access via OleDb.
    /// </summary>
    public class MicrosoftAccessDatabaseFormat : FileOutputFormat
    {
        /// <summary>
        /// Instruction if you get problem with the 64-bit Access Engine with Office 32-bit remember you need to install Access Database Engine
        /// and convert Access 32 bit to Access 64 Bit Access Database Engine. Please follow the instruction
        /// https://screencast.autodesk.com/Main/Details/7132d430-dc09-40bc-9148-94ff9db41c24
        /// http://techblog.aimms.com/2014/10/27/installing-32-bit-and-64-bit-microsoft-access-drivers-next-to-each-other/
        /// </summary>
        
        
        
        private OleDbConnection _cnn;
        private OleDbCommand _insertCommand;

        /// <summary>
        /// list of the names of each of the fields found in the SqlDataReader, this becomes populated when WriteHeaders is called 
        /// and is used to setup the Access table as well as to get values out of the SqlDataReader in Append(DataRow r)
        /// </summary>
        private List<string> _listField;
        private List<string> _listDataType;
        

        public MicrosoftAccessDatabaseFormat(string outputFilename): base(outputFilename)
        {
        }

        public override string GetFileExtension()
        {
            return ".accdb";
        }

        public override void Open()
        {
            
            //create an Access database (empty) - or later on maybe see one already exists 

            // if Database is exists then delete the database and create new database
            if (File.Exists(OutputFilename))
                File.Delete(OutputFilename);

            //Access data OLEDB provider and path
            try
            {
                Catalog cat = new Catalog();
                cat.Create("Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + OutputFilename + ";" + "Jet OLEDB:Engine Type=5");
                
            }
            catch (Exception e)
            {
                throw new Exception("Could not create/open Microsoft Access OLE Database Provider to write to database, maybe it isn't installed?",e);
            }

        }

        public override void WriteHeaders(DataTable t)
        {
            //Find the table name from the path of file.

            string tableName = Path.GetFileName(OutputFilename);
            tableName = tableName.Substring(0, tableName.LastIndexOf("."));

            //Collect all sqldatabase Fields Name and datatype for Access create table and insert the data tinto Access Database table
           
            int i = 0;
            _listField = new List<string>();
            _listDataType = new List<string>();
            while (i < t.Columns.Count)
            {
                _listField.Add(t.Columns[i].ColumnName);
                _listDataType.Add(t.Columns[i].DataType.ToString());
                i++;
             
            }
            
            //Join all data fields and data types for create Access table and insert data into Access Database table.
            
            string allFields = string.Join(",", _listField.ToArray());
            string allFieldsValues = "@" + string.Join(",@", _listField.ToArray());

            
            string CreateTableFields = "";
            for (int x = 0; x < _listField.Count; x++)
            {
                if (_listField[x] == "PROCHI")
                {
                    _listDataType[x] =  "TEXT(10)";

                }

                CreateTableFields = CreateTableFields + "[" + _listField[x] + "]" + _listDataType[x] + ",";
            }

            CreateTableFields = CreateTableFields.Remove(CreateTableFields.Length - 1);
          
            //====================================================================

            
            //====================Create Table into Access Database
            string cs = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + OutputFilename + ";" + "Jet OLEDB:Engine Type=5";
            string strSQL = "CREATE TABLE " + tableName + " (" + CreateTableFields + ")";
            _cnn = new OleDbConnection(cs);
            _cnn.Open();

            OleDbCommand cmd1 = new OleDbCommand();
                
            cmd1.CommandText = strSQL;
            cmd1.CommandType = CommandType.Text;
            cmd1.Connection = _cnn;
            
            //send create tables command
            cmd1.ExecuteNonQuery();
            
            //setup INSERT command
            _insertCommand = new OleDbCommand();
            _insertCommand.Connection = _cnn;

            _insertCommand.CommandText = @"INSERT INTO " + tableName + " ( " + allFields + " ) VALUES ( " + allFieldsValues + " )";
          
        }

        public override void Append(DataRow r)
        {
            //Insert data into access database
            _insertCommand.Parameters.Clear();
            for (int x = 0; x < r.Table.Columns.Count; x++)
                _insertCommand.Parameters.AddWithValue("@" + _listField[x] + " ", r["" + _listField[x] + ""]);

            _insertCommand.ExecuteNonQuery();
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
           _cnn.Close();
        }

    }
}
