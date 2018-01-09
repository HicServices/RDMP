using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// See IManagedConnection
    /// </summary>
    public class ManagedConnection : IManagedConnection
    {
        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public IManagedTransaction ManagedTransaction { get; private set; }
        public Thread OriginThread { get; private set; }

        private bool hadToOpenConnection = false;

        public ManagedConnection(DiscoveredServer discoveredServer, IManagedTransaction managedTransaction)
        {
            OriginThread = Thread.CurrentThread;

            //get a new connection or use the existing one within the transaction
            Connection = discoveredServer.GetConnection(managedTransaction);

            //if there is a transaction, also store the transaction
            ManagedTransaction = managedTransaction;
            Transaction = managedTransaction != null ? managedTransaction.Transaction : null;

            //if there isn't a transaction then we opened a new connection so we had better remember to close it again
            if(managedTransaction == null)
            {
                hadToOpenConnection = true;
                Debug.Assert(Connection.State == ConnectionState.Closed);
                Connection.Open();
            }
            else
            {
                if(OriginThread != managedTransaction.OriginThread)
                    throw new Exception("Cannot open new connection on Thread " + OriginThread.ManagedThreadId + "(" + OriginThread.Name + ")" + " because there is an ongoing Transaction using the connection on Thread " + managedTransaction.OriginThread + "(" + managedTransaction.OriginThread.Name + ")");
            }

            
        }

        public void Dispose()
        {
            if (hadToOpenConnection)
                Connection.Dispose();
        }
    }
}