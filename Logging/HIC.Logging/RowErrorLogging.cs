using System;
using System.Data;
using System.Data.SqlClient;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// Static class for logging individual row errors that occur during data load to the logging database (this is a bad idea since you will end up with a logging
    /// database that is bigger than your main datasets depending on how many errors / runs you have).
    /// </summary>
    [Obsolete("Logging every row error in the logging database is a bad idea and since it will bloat your logging database size massively")]
    public class RowErrorLogging
    {
        private static RowErrorLogging _instance;
        private static readonly object instanceLock = new object();

        private DiscoveredServer _databaseSettings;

        internal DiscoveredServer DatabaseSettings
        {
            get { return _databaseSettings; }
        }


        private object oLock = new object();

        private RowErrorLogging()
        {
        }

        /// <summary>
        /// Get a reference to the singleton RowErrorLogging class instance, If you need to change the database settings used by this class, use DatabseSettings property
        /// </summary>
        /// <returns></returns>
        public static RowErrorLogging GetInstance()
        {
            //lock so that we do not call the constructor more than once (if multi threading)
            lock (instanceLock)
                if (_instance == null)
                    _instance = new RowErrorLogging();

            return _instance;
        }

        public enum RowErrorType
        {

            LoadRow,
            Duplication,
            Validation,
            DatabaseOperation,
            Unknown

        }

        /// <summary>
        /// Records that a data load row failed to load into the database.
        /// </summary>
        /// <param name="tableLoadInfo">Class holding information about the ongoing data load</param>
        /// <param name="typeOfError">The general type of the error e.g. ValidationFailed could be for a dodgy postcode</param>
        /// <param name="description">Full description of why the row is not inserted (including any Exception messages) </param>
        /// <param name="locationOfRow">The full name of a data table or flat file that contains the row</param>
        /// <param name="requiresReloading">true if the load needs to be re-run on this row, false if not e.g. the row is a duplicate</param>
        public void LogRowError(ITableLoadInfo tableLoadInfo, RowErrorType typeOfError, string description, string locationOfRow, bool requiresReloading, string columnName = null)
        {
            _databaseSettings = tableLoadInfo.DatabaseSettings;

            //only send one query at once
            lock (oLock)
            {
                //if (con == null || con.State == ConnectionState.Closed)
                //    refreshConnection();

                //increase the number of errors so far by 1
                tableLoadInfo.IncrementErrorRows();

                int typeID = GetTypeIDFromLookupTable(typeOfError);

                SqlConnection con = (SqlConnection) _databaseSettings.GetConnection();
                
                SqlCommand cmdInsertErrorRow = null;
                
                try
                {
                    if (columnName != null)
                    {
                        cmdInsertErrorRow =
                            new SqlCommand(
                                "INSERT INTO RowError (tableLoadRunID,rowErrorTypeID,description,locationOfRow,requiresReloading,columnName) VALUES  (@tableLoadRunID,@rowErrorTypeID,@description,@locationOfRow,@requiresReloading,@columnName)",
                                con);
                        cmdInsertErrorRow.Parameters.Add("@columnName", SqlDbType.VarChar, -1);
                        cmdInsertErrorRow.Parameters["@columnName"].Value = columnName;
                    }
                    else
                    {
                        cmdInsertErrorRow =
                            new SqlCommand(
                                "INSERT INTO RowError (tableLoadRunID,rowErrorTypeID,description,locationOfRow,requiresReloading) VALUES  (@tableLoadRunID,@rowErrorTypeID,@description,@locationOfRow,@requiresReloading)",
                                con);
                    }

                    cmdInsertErrorRow.Parameters.Add("@tableLoadRunID", SqlDbType.Int);
                    cmdInsertErrorRow.Parameters.Add("@rowErrorTypeID", SqlDbType.Int);
                    cmdInsertErrorRow.Parameters.Add("@description", SqlDbType.VarChar, -1);
                    cmdInsertErrorRow.Parameters.Add("@locationOfRow", SqlDbType.VarChar, -1);
                    cmdInsertErrorRow.Parameters.Add("@requiresReloading", SqlDbType.Bit);

                    cmdInsertErrorRow.Parameters["@tableLoadRunID"].Value = tableLoadInfo.ID;
                    cmdInsertErrorRow.Parameters["@rowErrorTypeID"].Value = typeID;
                    cmdInsertErrorRow.Parameters["@description"].Value = description;
                    cmdInsertErrorRow.Parameters["@locationOfRow"].Value = locationOfRow;
                    cmdInsertErrorRow.Parameters["@requiresReloading"].Value = requiresReloading;

                    //if (con.State != ConnectionState.Open)
                    con.Open();

                    cmdInsertErrorRow.ExecuteNonQuery(); // fire and remember...
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error inserting error row into database:" + ex.Message + " connection state was " + con.State +
                        " connection string was " + con.ConnectionString, ex);
                }
                finally
                {
                    if (cmdInsertErrorRow != null)
                        cmdInsertErrorRow.Dispose();
                    if (con != null)
                        con.Dispose();
                }

            }
        }

        private int GetTypeIDFromLookupTable(RowErrorType typeOfError)
        {
            return (int)typeOfError + 1;
            //try
            //{
            //    lock (oLock)
            //    {
            //        if (con.State != ConnectionState.Open)
            //            con.Open();

            //        //look up the ID based on the value in the enum (enum name must match lookup name in database)
            //        string typeOfErrorAsString = Enum.GetName(typeof(RowErrorType), typeOfError);

            //        SqlCommand cmdLookupTypeID = new SqlCommand("SELECT ID from z_RowErrorType WHERE type=@type", con);
            //        cmdLookupTypeID.Parameters.Add("@type", SqlDbType.NChar, 20);
            //        cmdLookupTypeID.Parameters["@type"].Value = typeOfErrorAsString;

            //        return int.Parse(cmdLookupTypeID.ExecuteScalar().ToString());
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw new Exception("Failed to lookup the ID of RowErrorType enum " + typeOfError + ", perhaps it is missing from the database?" + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
            //    throw;
            //}
            //finally
            //{
            //    con.Close();
            //}

        }
    }
}
