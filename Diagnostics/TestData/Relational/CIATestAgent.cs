using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DataTableExtension;

namespace Diagnostics.TestData.Relational
{
    public class CIATestAgent
    {
        public int PKAgentID;
        public string AgentCodeName;
        public List<CIATestAgentEquipment> Equipment { get; set; }

        public CIATestAgent(Random r,List<CIATestAgent> avoidDuplicationWith)
        {
            int id = 1;

            while (avoidDuplicationWith.Any(agent => agent.PKAgentID == id))
            {
                id = r.Next(100000);
            }

            PKAgentID = id;
            AgentCodeName = TestPerson.GetRandomSurname(r);

            Equipment = new List<CIATestAgentEquipment>();

            for (int i = 0; i < r.Next(4); i++)
                Equipment.Add(new CIATestAgentEquipment(r, this));
        }

        public void AddColumnsToDataTable(DataTable dt)
        {
            dt.Columns.Add("PKAgentID");
            dt.Columns.Add("AgentCodeName");
        }

        public void AddToDataTable(DataTable dt)
        {
            dt.Rows.Add(new object[] { PKAgentID, AgentCodeName});
        }

        private bool isCommited = false;

        public void CommitToDatabase(DiscoveredDatabase database, DbConnection con)
        {
            if(isCommited)
                return;

            isCommited = true;

            //commit the Equipment
            DataTable dtEquipment = new DataTable();
            CIATestAgentEquipment.AddColumnsToDataTable(dtEquipment);
            
            foreach(CIATestAgentEquipment eq in Equipment)
                eq.AddToDataTable(dtEquipment);

            SqlBulkCopy bulk = new SqlBulkCopy((SqlConnection) con);
            bulk.DestinationTableName = "CIATestAgentEquipment";

            foreach (DataColumn col in dtEquipment.Columns)
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

            UsefulStuff.BulkInsertWithBetterErrorMessages(bulk, dtEquipment, database.Server);

            //add the agent
            database.Server.GetCommand(string.Format("INSERT INTO CIATestAgent VALUES ({0},'{1}')",PKAgentID,AgentCodeName),con).ExecuteNonQuery();
        }
    }
}