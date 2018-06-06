using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;


namespace ReusableLibraryCode
{
    /// <summary>
    /// Contains lots of generically useful static methods
    /// </summary>
    public class UsefulStuff
    {
        private static UsefulStuff _instance;
        public static Regex RegexThingsThatAreNotNumbersOrLetters = new Regex("[^0-9A-Za-z]+");
        public static Regex RegexThingsThatAreNotNumbersOrLettersOrUnderscores = new Regex("[^0-9A-Za-z_]+");
        
        public static UsefulStuff GetInstance()
        {
            if (_instance == null)
                _instance = new UsefulStuff();

            return _instance;
        }
        
        public int GetColumnLength(DbConnection connection, string tableName, string columnName)
        {
            if (connection is MySqlConnection)
                return GetColumnLengthMySql(connection, tableName, columnName);

            tableName = tableName.Trim(new char[]{'[',']'});
            columnName = columnName.Trim(new char[] { '[', ']' });

            bool hadToOpen = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                hadToOpen = true;
            }

            try
            {
                using (DbCommand cmd = DatabaseCommandHelper.GetCommand("sp_columns", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    DatabaseCommandHelper.AddParameterWithValueToCommand("@table_name", cmd, tableName);
                    using (DbDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            if (r["COLUMN_NAME"].ToString() == columnName)
                            {
                                return Int32.Parse(r["PRECISION"].ToString()); // deals with weird mssql datatypes like decimal
                            }
                        }
                    }
                }
            }
            finally
            {
                if(hadToOpen)
                    connection.Close();
            }
            
            
            throw new Exception("Could not find column named " + columnName + " in table " + tableName );
        }

        public int GetColumnLengthMySql(DbConnection connection,string tableName, string columnName)
        {
            tableName = tableName.Trim('`');

            bool hadToOpen = false;
            if(connection.State != ConnectionState.Open)
            {
                hadToOpen = true;
                connection.Open();
            }

            DbCommand cmd = DatabaseCommandHelper.GetCommand("DESCRIBE `" + tableName + "`", connection);
            DbDataReader r = cmd.ExecuteReader();

            try
            {
                while (r.Read())
                    if (((string)r["Field"]).Equals(columnName))
                    {
                        string type = r["Type"] as string;

                        if (type.Equals("longtext"))
                            return 50000;

                        Match result = Regex.Match(type, "\\([0-9]+\\)");

                        if(result.Success)
                            return Int32.Parse(result.Value.Trim('(',')'));
                        else
                            throw new FormatException("According to MySQL type of column " + columnName + " is " + type + ", we cannot determine what the fields length is.");
                    }
            }
            finally
            {
                r.Close();

                //leave it in the same state we received it in
                if(hadToOpen)
                    connection.Close();
            }
            throw new MissingFieldException("Could not find field " + columnName + " in table " + tableName);
        }
        
        public string GetColumnTypeMySql(string connectionString, string databaseName, string tableName, string columnName)
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            
            databaseName = databaseName.Trim('`');
            tableName = tableName.Trim('`');

            con.Open();

            DbCommand cmd = DatabaseCommandHelper.GetCommand("DESCRIBE `" + databaseName + "`.`" + tableName + "`", con);
            DbDataReader r = cmd.ExecuteReader();

            try
            {
                while (r.Read())
                    if( ((string) r["Field"]).Equals(columnName))
                        return r["Type"] as string;
            }
            finally
            {
                r.Close();
                con.Close();   
            }
            throw new MissingFieldException("Could not find field " + columnName + " in table " + tableName);
        }

        public string GetColumnCollationType(string connectionString, string tableName, string columnName)
        {
            if (!connectionString.Contains("Initial Catalog") && !connectionString.Contains("Database"))
                throw new Exception("You need to set 'Initial Catalog' in order to use GetColumnLength method in UsefulStuff");

            tableName = tableName.Trim(new char[] { '[', ']' });
            columnName = columnName.Trim(new char[] { '[', ']' });

            //list the tables on in the database 
            DbConnection con = new SqlConnection(connectionString);


            try
            {
                con.Open();

                DbCommand cmd =  DatabaseCommandHelper.GetCommand(@"select c.collation_name FROM 
    sys.columns c 
INNER JOIN 
    sys.tables t ON c.object_id = t.object_id
INNER JOIN 
    sys.types ty ON c.system_type_id = ty.system_type_id  
WHERE 
t.name = @table_name AND
c.name = @column_name", con);


                DatabaseCommandHelper.AddParameterWithValueToCommand("@table_name", cmd, tableName);
                DatabaseCommandHelper.AddParameterWithValueToCommand("@column_name", cmd, columnName);
                

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return null;
                
                return result.ToString();

            }
            finally
            {
                con.Close();
            }
            
        }
        
        public string GetColumnCollationTypeMySql(string connectionString,string databaseName, string tableName, string columnName)
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            
            databaseName = databaseName.Trim('`');
            tableName = tableName.Trim('`');
            con.Open();

        //SHOW FULL COLUMNS FROM sprintfourdatacatalogue.catalogue
            DbCommand cmd = DatabaseCommandHelper.GetCommand("SHOW FULL COLUMNS FROM  `" + databaseName + "`.`" + tableName + "`", con);
            DbDataReader r = cmd.ExecuteReader();

            try
            {
                while (r.Read())
                    if( ((string) r["Field"]).Equals(columnName))
                        return r["Collation"] as string;
            }
            finally
            {
                r.Close();
                con.Close();   
            }
            throw new MissingFieldException("Could not find field " + columnName + " in table " + tableName);
        }

        public string[] ListStoredProcedures(string connectionString,string databaseName )
        {
            databaseName = databaseName.Trim(new char[] {'[', ']'});

            List<string> procs = new List<string>();

            //list the tables on in the database 
            DbConnection con = new SqlConnection(connectionString);
            con.Open();

            DbCommand cmd =  DatabaseCommandHelper.GetCommand("use ["+databaseName+"]; exec sp_stored_procedures ", con);
            
            
            DbDataReader r = cmd.ExecuteReader();


            while (r.Read())
            {
                //these come back with ;x where x is the number of parameters for some unknown reason
                string procName = (string) r["PROCEDURE_NAME"];
                procName = procName.Split(new char[]{';'})[0];
                procs.Add(procName);
            }

            
            con.Close();

            return procs.ToArray();
        }
        
        public string ValidateQuery(string connectionString, string query)
        {
            if (!query.StartsWith("SELECT"))
                throw new Exception("ValidateQuery expects a string that starts with SELECT");

            //list the tables on in the database 
            DbConnection con = new SqlConnection(connectionString);
            con.Open();

            DbTransaction t = con.BeginTransaction(IsolationLevel.ReadCommitted);

            DbCommand cmdParseOn =  DatabaseCommandHelper.GetCommand("SET PARSEONLY ON", con,t);
            DbCommand cmdToTest =  DatabaseCommandHelper.GetCommand(query, con,t);
            DbCommand cmdParseOff =  DatabaseCommandHelper.GetCommand("SET PARSEONLY OFF", con,t);


            cmdParseOn.ExecuteNonQuery();
            string result = (string)cmdToTest.ExecuteScalar();
            
            if (result == null)
                return "The SQL syntax is valid";

            cmdParseOff.ExecuteNonQuery();

            t.Rollback();

            con.Close();

            return result;
        }

        internal DateTime LooseConvertToDate(object iO)
        {
            DateTime sDate;

            try
            {
                sDate = Convert.ToDateTime(iO);
            }
            catch (FormatException)
            {
                try
                {
                    string sDateAsString = iO.ToString();

                    if (sDateAsString.Length == 8)
                    {
                        sDateAsString = sDateAsString.Substring(0, 2) + "/" + sDateAsString.Substring(2, 2) + "/" +
                                        sDateAsString.Substring(4, 4);
                        sDate = Convert.ToDateTime(sDateAsString);
                    }
                    else if (sDateAsString.Length == 6)
                    {
                        sDateAsString = sDateAsString.Substring(0, 2) + "/" + sDateAsString.Substring(2, 2) + "/" +
                                        sDateAsString.Substring(4, 2);
                        sDate = Convert.ToDateTime(sDateAsString);
                    }
                    else
                        throw;
                }
                catch (Exception)
                {
                    throw new Exception("Cannot recognise date format :" + iO);

                }
            }
            return sDate;
        }


        public IEnumerable<string> GetArrayOfColumnNamesFromStringPastedInByUser(string text)
        {
            
            if (String.IsNullOrWhiteSpace(text))
                yield break;

            string[] split = text.Split(new char[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

            
            //identifies the last word in a collection of multiple words (requires you .Trim() so we dont get ending whitespace match)
            Regex regexLastWord = new Regex("\\s[^\\s]*$");
            foreach (string s in split)
            {
                //clean the string

                string toAdd = s.Trim();
                if (toAdd.Contains("."))
                    toAdd = toAdd.Substring(toAdd.LastIndexOf(".") + 1);

                toAdd = toAdd.Replace("]", "");
                toAdd = toAdd.Replace("[", "");

                toAdd = toAdd.Replace("`", "");

                if (String.IsNullOrWhiteSpace(toAdd))
                    continue;

                if (regexLastWord.IsMatch(toAdd))
                    toAdd = regexLastWord.Match(toAdd).Value.Trim();

                yield return toAdd;
            }
        }

        public bool CHIisOK(string sCHI)
        {
            long n;
            DateTime d;
            bool ok = false;

            if (Int64.TryParse(sCHI, out n) && sCHI.Length == 10)
            {
                string sDate = sCHI.Substring(0, 2) + "/" + sCHI.Substring(2, 2) + "/" + sCHI.Substring(4, 2);
                string sCheck = sCHI.Substring(sCHI.Length - 1);
                if (DateTime.TryParse(sDate, out d) && GetCHICheckDigit(sCHI) == sCheck)
                    ok = true;
            }

            return ok;
        }

        public string GetCHICheckDigit(string sCHI)
        {
            int sum = 0, c = 0, lsCHI = 0;

            //sCHI = "120356785";
            lsCHI = sCHI.Length; // Must be 10!!

            sum = 0;
            c = (int)'0';
            for (int i = 0; i < lsCHI - 1; i++)
                sum += ((int)(sCHI.Substring(i, 1)[0]) - c) * (lsCHI - i);
            sum = sum % 11;

            c = 11 - sum;
            if (c == 11) c = 0;

            return ((char)(c + (int)'0')).ToString();

        }

        public static string MD5File(string filename, int retryCount = 6)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream));
                    }
                }
            }
            catch (IOException ex)
            {
                //couldn't access the file so wait 5 seconds then try again
                Thread.Sleep(1000);

                if (retryCount-- > 0)
                    return MD5File(filename, retryCount);//try it again (recursively)
                else
                    throw ex;
            }


        }

        public static string MD5Stream(MemoryStream stream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(stream));
            }
        }
        
     
        public static bool RequiresLength(string columnType)
        {
            columnType = columnType.ToLower();

            switch (columnType)
            {
                case "binary": return true;
                case "bit": return false;
                case "char": return true;
                case "image": return true;
                case "nchar": return true;
                case "nvarchar": return true;
                case "varbinary": return true;
                case "varchar": return true;
                case "numeric": return true;

                default: return false;
            }
        }

        public static bool HasPrecisionAndScale(string columnType)
        {
            columnType = columnType.ToLower();

            switch (columnType)
            {
                case "decimal": return true;
                case "numeric": return true;
                default: return false;
            }
        }
        
        public Dictionary<string, object> DbDataReaderToDictionary(DbDataReader r)
        {
            var dict = new Dictionary<string, object>();

            for (int lp = 0; lp < r.FieldCount; lp++)
            {
                object value = r.GetValue(lp);
                value = value == DBNull.Value ? null : value;
                dict.Add(r.GetName(lp), value);
            }
            return dict;
        }
        
        public static string RemoveIllegalFilenameCharacters(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                foreach (char invalidFileNameChar in System.IO.Path.GetInvalidFileNameChars())
                    value = value.Replace(invalidFileNameChar.ToString(), "");

            return value;
        }

        
        public static void ExecuteBatchNonQuery(string sql, DbConnection conn, DbTransaction transaction = null, int timeout = 30)
        {
            Dictionary<int, Stopwatch> whoCares;
            ExecuteBatchNonQuery(sql, conn, transaction, out whoCares, timeout);
        }

        /// <summary>
        /// Executes the given SQL against the database + sends GO delimited statements as separate batches
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <param name="performanceFigures">Line number the batch started at and the time it took to complete it</param>
        public static void ExecuteBatchNonQuery(string sql, DbConnection conn, DbTransaction transaction,out Dictionary<int, Stopwatch> performanceFigures, int timeout = 30) 
        {
            performanceFigures = new Dictionary<int, Stopwatch>();

            string sqlBatch = string.Empty;
            DbCommand cmd = DatabaseCommandHelper.GetCommand(string.Empty, conn, transaction);
            bool hadToOpen = false;

            if (conn.State != ConnectionState.Open)
            {

                conn.Open();
                hadToOpen = true;
            }

            int lineNumber = 1;

            sql += "\nGO";   // make sure last batch is executed.
            try
            {
                foreach (string line in sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    lineNumber++;

                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        if (string.IsNullOrWhiteSpace(sqlBatch))
                            continue;

                        if (!performanceFigures.ContainsKey(lineNumber))
                            performanceFigures.Add(lineNumber, new Stopwatch());
                        performanceFigures[lineNumber].Start();

                        cmd.CommandText = sqlBatch;
                        cmd.CommandTimeout = timeout;
                        cmd.ExecuteNonQuery();

                        performanceFigures[lineNumber].Stop();
                        sqlBatch = string.Empty;
                    }
                    else
                    {
                        sqlBatch += line + "\n";
                    }
                }
            }
            finally
            {
                if (hadToOpen)
                    conn.Close();
            }
        }


        public string[] ListTablesOracle(OracleConnectionStringBuilder builder)
        {
            OracleConnection con = new OracleConnection(builder.ConnectionString);
            con.Open();
            try
            {
                return ListTablesOracle(con).ToArray();
            }
            finally
            {
                con.Close();   
            }
            
        }
        /// <summary>
        /// Requires an open and available connection.  Displays all the tables that the current user can access
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private string[] ListTablesOracle(OracleConnection con)
        {
            OracleCommand cmd = new OracleCommand(@"SELECT table_name FROM all_tables where owner not in 
(
'SYS','SYSTEM','MDSYS','OUTLN','CTXSYS','OLAPSYS','FLOWS_FILES','DVSYS','AUDSYS','DBSNMP','GSMADMIN_INTERNAL','OJVMSYS','ORDSYS','APPQOSSYS','XDB','ORDDATA','WMSYS','LBACSYS'
)", con);

            OracleDataReader r = cmd.ExecuteReader();

            List<string> toReturn = new List<string>();
            while (r.Read())
                toReturn.Add((string) r["table_name"]);

            return toReturn.ToArray();
        }

        
        public static int BulkInsertWithBetterErrorMessages(SqlBulkCopy insert, DataTable dt, DiscoveredServer serverForLineByLineInvestigation)
        {
            int rowsWritten = 0;

            EmptyStringsToNulls(dt);

            InspectDataTableForFloats(dt);
            
            try
            {
                //send data read to server
                insert.WriteToServer(dt);
                rowsWritten += dt.Rows.Count;

                return rowsWritten;
            }
            catch (Exception e)
            {
                //user does not want to replay the load one line at a time to get more specific error messages
                if (serverForLineByLineInvestigation == null)
                    throw;

                int line = 1;
                string baseException = ExceptionHelper.ExceptionToListOfInnerMessages(e, true);
                baseException = baseException.Replace(Environment.NewLine, Environment.NewLine + "\t");
                baseException = Environment.NewLine + "First Pass Exception:" + Environment.NewLine + baseException;
                baseException += Environment.NewLine + "Second Pass Exception:";

                DiscoveredColumn[] destinationColumnsContainedInMapping;

                try
                {
                    var dest = serverForLineByLineInvestigation.GetCurrentDatabase().ExpectTable(insert.DestinationTableName);
                    destinationColumnsContainedInMapping = dest.DiscoverColumns().Where(c => insert.ColumnMappings.Cast<SqlBulkCopyColumnMapping>().Any(m=>m.DestinationColumn==c.GetRuntimeName())).ToArray();
                }
                catch (Exception )
                {
                    throw e; //couldn't even enumerate the destination columns, whatever the original Exception was it must be serious, just rethrow it
                }

                //have to use a new object because current one could have a broken transaction associated with it
                using( var con = (SqlConnection)serverForLineByLineInvestigation.GetConnection())
                {
                    con.Open();
                    SqlTransaction investigationTransaction = con.BeginTransaction("Investigate BulkCopyFailure");
                    SqlBulkCopy investigationOneLineAtATime = new SqlBulkCopy(con, SqlBulkCopyOptions.KeepIdentity, investigationTransaction);
                    investigationOneLineAtATime.DestinationTableName = insert.DestinationTableName;
                    
                    foreach (SqlBulkCopyColumnMapping m in insert.ColumnMappings)
                        investigationOneLineAtATime.ColumnMappings.Add(m);

                    //try a line at a time
                    foreach (DataRow dr in dt.Rows)
                        try
                        {
                            investigationOneLineAtATime.WriteToServer(new[] { dr }); //try one line
                            line++;
                        }
                        catch (Exception exception)
                        {
                            Regex columnLevelComplaint = new Regex("bcp client for colid (\\d+)");

                            Match match = columnLevelComplaint.Match(exception.Message);
                            
                            if (match.Success)
                            {
                                //it counts from 1 because its stupid, hence to get our column index we subtract 1 from whatever column it is complaining about
                                int columnItHates = Convert.ToInt32(match.Groups[1].Value) - 1;

                                if(destinationColumnsContainedInMapping.Length > columnItHates)
                                {
                                    var offendingColumn = destinationColumnsContainedInMapping[columnItHates];
                                    
                                    if(dt.Columns.Contains(offendingColumn.GetRuntimeName()))
                                    {
                                        var sourceValue = dr[offendingColumn.GetRuntimeName()];
                                        throw new FileLoadException(baseException + "BulkInsert complained on data row " + line + " the complaint was about column number " + columnItHates + ": <<" + offendingColumn.GetRuntimeName() + ">> which had value <<" + sourceValue + ">> destination data type was <<" + offendingColumn.DataType +">>", exception);
                                    }
                                }
                            }

                            throw new FileLoadException(
                                baseException + "Failed to load data row " + line + " the following values were rejected by the database: " +
                                Environment.NewLine + dr.ItemArray.Aggregate((o, n) => o + Environment.NewLine + n),
                                exception);
                        }
                
                    //it worked... how!?
                    investigationTransaction.Rollback();
                    con.Close();

                    throw new Exception(baseException + "Bulk insert failed but when we tried to repeat it a line at a time it worked... very strange", e);
                }
              
            }
        }

        private static void EmptyStringsToNulls(DataTable dt)
        {
            foreach (var col in dt.Columns.Cast<DataColumn>().Where(c => c.DataType == typeof (string)))
                foreach (DataRow row in dt.Rows)
                {
                    var o = row[col];

                    if (o == DBNull.Value || o == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(o.ToString()))
                        row[col] = DBNull.Value;
                }
        }

        private static void InspectDataTableForFloats(DataTable dt)
        {
            //are there any float or float? columns
            DataColumn[] floatColumns = dt.Columns.Cast<DataColumn>()
                .Where(
                    c => c.DataType == typeof (float) || c.DataType == typeof (float?)).ToArray();

            if(floatColumns.Any())
                throw new NotSupportedException("Found float column(s) in data table, SQLServer does not support floats in bulk insert, instead you should use doubles otherwise you will end up with the value 0.85 turning into :0.850000023841858 in your database.  Float column(s) were:" + string.Join(",",floatColumns.Select(c=>c.ColumnName).ToArray()));

            //are there any object columns
            var objectColumns = dt.Columns.Cast<DataColumn>().Where(c => c.DataType == typeof (object)).ToArray();

            //do any of the object columns have floats or float? in them?
            for (int i = 0; i < Math.Min(100, dt.Rows.Count); i++)
                foreach (DataColumn c in objectColumns)
                {
                    object value = dt.Rows[i][c.ColumnName];

                    Type underlyingType = value.GetType();
                    
                     if (underlyingType == typeof(float) || underlyingType == typeof(float?))
                        throw new NotSupportedException("Found float value " + value + " in data table, SQLServer does not support floats in bulk insert, instead you should use doubles otherwise you will end up with the value 0.85 turning into :0.850000023841858 in your database");
                }
        }

        public static StackTrace GetStackTrace(Thread targetThread)
        {
            using (ManualResetEvent fallbackThreadReady = new ManualResetEvent(false), exitedSafely = new ManualResetEvent(false))
            {
                Thread fallbackThread = new Thread(delegate()
                {
                    fallbackThreadReady.Set();
                    while (!exitedSafely.WaitOne(200))
                    {
                        try
                        {
                            targetThread.Resume();
                        }
                        catch (Exception) {/*Whatever happens, do never stop to resume the target-thread regularly until the main-thread has exited safely.*/}
                    }
                });
                fallbackThread.Name = "GetStackFallbackThread";
                try
                {
                    fallbackThread.Start();
                    fallbackThreadReady.WaitOne();
                    //From here, you have about 200ms to get the stack-trace.
                    targetThread.Suspend();
                    StackTrace trace = null;
                    try
                    {
                        trace = new StackTrace(targetThread, true);
                    }
                    catch (ThreadStateException)
                    {
                        //failed to get stack trace, since the fallback-thread resumed the thread
                        //possible reasons:
                        //1.) This thread was just too slow (not very likely)
                        //2.) The deadlock ocurred and the fallbackThread rescued the situation.
                        //In both cases just return null.
                    }
                    try
                    {
                        targetThread.Resume();
                    }
                    catch (ThreadStateException) {/*Thread is running again already*/}
                    return trace;
                }
                finally
                {
                    //Just signal the backup-thread to stop.
                    exitedSafely.Set();
                    //Join the thread to avoid disposing "exited safely" too early. And also make sure that no leftover threads are cluttering iis by accident.
                    fallbackThread.Join();
                }
            }
        }

        /// <summary>
        /// Locates a manifest resource in the assembly under the manifest name subspace.  If you want to spray the resource MySoftwareSuite.MyApplication.MyResources.Bob.txt then pass:
        /// 1. the assembly containing the resource (e.g. typeof(MyClass1).Assembly)
        /// 2. the full path to the resource file: "MySoftwareSuite.MyApplication.MyResources.Bob.txt"
        /// 3. the filename "Bob.txt"
        /// </summary>
        /// <param name="assembly">The dll e.g. MySoftwareSuite.MyApplication.dll</param>
        /// <param name="manifestName">The full path to the manifest resource e.g. MySoftwareSuite.MyApplication.MyResources.Bob.txt</param>
        /// <param name="file">The filename ONLY of the resource e.g. Bob.txt</param>
        public static FileInfo SprayFile(Assembly assembly, string manifestName, string file)
        {
            Stream fileToSpray = assembly.GetManifestResourceStream(manifestName);

            if (fileToSpray == null)
                throw new Exception("assembly.GetManifestResourceStream returned null for manifest name " + manifestName + " in assembly " + assembly);

            //get the bytes
            byte[] buffer = new byte[fileToSpray.Length];
            fileToSpray.Read(buffer, 0, buffer.Length);

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pathToDataApp = Path.Combine(folder, "RDMP");
            if (!Directory.Exists(pathToDataApp))
                Directory.CreateDirectory(pathToDataApp);

            FileInfo target = new FileInfo(Path.Combine(pathToDataApp,file));

            File.WriteAllBytes(target.FullName, buffer);

            fileToSpray.Close();
            fileToSpray.Dispose();
            
            return target;
        }

        public static string GetHumanReadableByteSize(long len)
        {
            string[] sizes = { "bytes", "KB", "MB", "GB" };
            
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public static bool VerifyFileExists(string path, int timeout)
        {
            var task = new Task<bool>(() =>
            {
                try
                {
                    var fi = new FileInfo(path);
                    return fi.Exists;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            task.Start();
            return task.Wait(timeout) && task.Result;
        }

        public static bool VerifyFileExists(Uri uri, int timeout)
        {
            return VerifyFileExists(uri.LocalPath,timeout);

        }

        public string DataTableToHtmlDataTable(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<Table>");
            sb.Append("<TR>");

            foreach (DataColumn column in dt.Columns)
                sb.Append("<TD>" + column.ColumnName + "</TD>");

            sb.AppendLine("</TR>");

            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<TR>");

                foreach (var cellObject in row.ItemArray)
                    sb.Append("<TD>" + cellObject.ToString() + "</TD>");
                
                sb.AppendLine("</TR>");
            }

            sb.Append("</Table>");

            return sb.ToString();
        }
        public void ShowFolderInWindowsExplorer(DirectoryInfo directoryInfo)
        {
            string argument = " \"" + directoryInfo.FullName + "\"";
            Process.Start("explorer.exe", argument);
        }

        public void ShowFileInWindowsExplorer(FileInfo fileInfo)
        {
            string argument = "/select, \"" + fileInfo.FullName + "\"";
            Process.Start("explorer.exe", argument);
        }

        public string GetClipboardFormatedHtmlStringFromHtmlString(string s)
        {
            const int maxLength = 9999999;
            if(s.Length > maxLength)
                throw new ArgumentException("String s is too long, the maximum length is " + maxLength + " but argument s was length " + s.Length,"s");

            var guidStart = Guid.NewGuid().ToString().Substring(0,7);
            var guidEnd = Guid.NewGuid().ToString().Substring(0, 7);

            //one in a million that the first 7 digits of the guid are the same as one another or exist in the data string
            if (s.Contains(guidStart) || s.Contains(guidEnd) || guidStart.Equals(guidEnd))
                return GetClipboardFormatedHtmlStringFromHtmlString(s);//but possible I guess so try again

            string template = "Version:1.0\r\nStartHTML:"+guidStart + "\r\nEndHTML:" +guidEnd +"\r\n";

            s = template
              .Replace(guidStart, template.Length.ToString("0000000"))
              .Replace(guidEnd, (template.Length + s.Length).ToString("0000000")) + s;

            return s;
        }

        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static string PascalCaseStringToHumanReadable(string pascalCaseString)
        {
            return Regex.Replace(pascalCaseString, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
        }

        public void ConfirmContentsOfDirectoryAreTheSame(DirectoryInfo first, DirectoryInfo other)
        {
            if (first.EnumerateFiles().Count() != other.EnumerateFiles().Count())
                throw new Exception("found different number of files in Globals directory " + first.FullName + " and " + other.FullName);

            var filesInFirst = first.EnumerateFiles().ToArray();
            var filesInOther = other.EnumerateFiles().ToArray();

            for (int i = 0; i < filesInFirst.Length; i++)
            {
                FileInfo file1 = filesInFirst[i];
                FileInfo file2 = filesInOther[i];
                if (!file1.Name.Equals(file2.Name))
                    throw new Exception("Although there were the same number of files in Globals directories " + first.FullName + " and " + other.FullName + ", there were differing file names (" + file1.Name + " and " + file2.Name + ")");

                if (!UsefulStuff.MD5File(file1.FullName).Equals(UsefulStuff.MD5File(file2.FullName)))
                    throw new Exception("File found in Globals directory which has a different MD5 from another Globals file.  Files were \"" + file1.FullName + "\" and \"" + file2.FullName + "\"");
            }
        }
    }
}
