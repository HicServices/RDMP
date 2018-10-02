using System;
using System.Data.Common;
using System.Threading;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace ReusableLibraryCode
{
    /// <summary>
    /// See IManagedTransaction
    /// </summary>
    public class ManagedTransaction : IManagedTransaction
    {
        public Thread OriginThread { get; private set; }

        public DbConnection Connection { get; set; }
        public DbTransaction Transaction { get; set; }

        public ManagedTransaction(DbConnection connection, DbTransaction transaction)
        {
            OriginThread = Thread.CurrentThread;

            Connection = connection;
            Transaction = transaction;
        }

        public void AbandonAndCloseConnection()
        {
            try
            {
                Transaction.Rollback();
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't rollback transactiton, nevermind");
            }
            finally
            {
                Connection.Close();
                Connection.Dispose();
            }
        }

        public void CommitAndCloseConnection()
        {

            try
            {
                Transaction.Commit();
            }
            finally
            {
                Connection.Close();
                Connection.Dispose();
            }
        }
    }
}
