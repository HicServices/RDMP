using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using FAnsi.Discovery;
using ReusableLibraryCode.DataAccess;

namespace DataLoadEngine.DatabaseManagement
{
    /// <summary>
    /// Stores the location of all the databases (RAW, STAGING, LIVE) available during a Data Load (See LoadMetadata).
    /// </summary>
    public class StandardDatabaseHelper
    {
        public INameDatabasesAndTablesDuringLoads DatabaseNamer { get; set; }

        public Dictionary<LoadBubble, DiscoveredDatabase> DatabaseInfoList = new Dictionary<LoadBubble, DiscoveredDatabase>();

        //Constructor
        internal StandardDatabaseHelper(DiscoveredDatabase liveDatabase, INameDatabasesAndTablesDuringLoads namer,DiscoveredServer rawServer)
        {
            DatabaseNamer = namer;

            

            foreach (LoadBubble stage in new[] {LoadBubble.Raw, LoadBubble.Staging, LoadBubble.Live,})
            {
                var stageName = DatabaseNamer.GetDatabaseName(liveDatabase.GetRuntimeName(), stage);
                DatabaseInfoList.Add(stage, stage == LoadBubble.Raw ? rawServer.ExpectDatabase(stageName) : liveDatabase.Server.ExpectDatabase(stageName));
                
            }
        }


        // Indexer declaration.
        // If index is out of range, the temps array will throw the exception.
        public DiscoveredDatabase this[LoadBubble index]
        {
            get
            {
                return DatabaseInfoList[index];
            }
        }
    }
}