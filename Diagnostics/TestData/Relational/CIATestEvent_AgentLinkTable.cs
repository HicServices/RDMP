using System.Data;

namespace Diagnostics.TestData.Relational
{
    public class CIATestEvent_AgentLinkTable
    {
        public string PKAgencyCodename;
        public CIATestClearenceLevel PKClearenceLevel;
        public int PKAgentID;

        public void AddColumnsToDataTable(DataTable dt)
        {
            dt.Columns.Add("PKAgencyCodename");
            dt.Columns.Add("PKClearenceLevel");
            dt.Columns.Add("PKAgentID");
        }

        public void AddToDataTable(DataTable dt)
        {
            dt.Rows.Add(new object[] { PKAgencyCodename, PKClearenceLevel, PKAgentID });
        }
    }
}