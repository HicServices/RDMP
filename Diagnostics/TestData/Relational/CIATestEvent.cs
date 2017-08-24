using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace Diagnostics.TestData.Relational
{
    public class CIATestEvent
    {
        public string PKAgencyCodename { get; set; }
        public CIATestClearenceLevel PKClearenceLevel { get; set; }

        public string EventName { get; set; }
        public CIATestEventType TypeOfEvent { get; set; }

        public CIATestAgent[] AgentsInvolved;
        public CIATestReport[] Reports;

        public DateTime EstimatedEventDate { get; set; }
        private static List<CIATestReport> _allTestReports = new List<CIATestReport>();

        public CIATestEvent(Random r, CIATestAgent[] agentsOnCase, CIATestClearenceLevel clearence, List<CIATestEvent> avoidDuplicationWith, List<CIATestInformant> informants, DateTime startTime, DateTime endTime)
        {
            AgentsInvolved = agentsOnCase;

            //get a random date between the ranges
            EstimatedEventDate = TestPerson.GetRandomDateBetween(startTime, endTime, r).Date;
            
            PKClearenceLevel = clearence;

            var codename =  GetRandomCodename(r);

            while (avoidDuplicationWith.Any(e => e.PKClearenceLevel == clearence && e.PKAgencyCodename.Equals(codename)))
                codename = GetRandomCodename(r);

            PKAgencyCodename = codename;


            Reports = GenerateReports(r, informants);
        }
        
        private CIATestReport[] GenerateReports(Random r, List<CIATestInformant> informants)
        {
            var numberOfReports = r.Next(0, 3);
            List<CIATestReport> toReturn = new List<CIATestReport>();

            for (int i = 0; i < numberOfReports; i++)
            {
                var report = new CIATestReport(r, this, _allTestReports, informants);
                toReturn.Add(report);
                _allTestReports.Add(report);
            }

            return toReturn.ToArray();
        }

        private string GetRandomCodename(Random r)
        {
            int code = r.Next(0 , 100);
            
            if(code >=0 && code < 10)
                return "Fish" + (code%10);

            if (code >= 10 && code < 20)
                return "Hallibut" + (code % 10);

            if (code >= 20 && code < 30)
                return "Obsidian" + (code % 10);

            if (code >= 30 && code < 40)
                return "DarkShroud" + (code % 10);

            if (code >= 40 && code < 50)
                return "OperationTotalKillFest" + (code % 10);

            if (code >= 50 && code < 60)
                return "PresidentInDanger_C" + (code % 10);

            if (code >= 60 && code < 70)
                return "OMGAliens" + (code % 10);

            if (code >= 70 && code < 80)
                return "OMGPoliticalDissidents" + (code % 10);

            if (code >= 80 && code < 90)
                return "OccupieMissileSilo" + (code % 10);

            if (code >= 90 && code < 100)
                return "FusionReactorOverload" + (code % 10);

            throw new Exception("Unexpected random number too high");
        }


        public void AddColumnsToDataTable(DataTable dt)
        {
            dt.Columns.Add("PKAgencyCodename");
            dt.Columns.Add("PKClearenceLevel");
            dt.Columns.Add("EventName");
            dt.Columns.Add("TypeOfEvent");
            dt.Columns.Add("EstimatedEventDate");
        }

        public void AddToDataTable(DataTable dt)
        {
            dt.Rows.Add(new object[] {PKAgencyCodename, PKClearenceLevel,EventName,TypeOfEvent,EstimatedEventDate});
        }

        public void CommitToDatabase(DiscoveredServer server,SqlConnection con)
        {
            foreach (CIATestAgent agent in AgentsInvolved)
                agent.CommitToDatabase(server,con);

            new SqlCommand(
                string.Format("INSERT INTO CIATestEvent VALUES ('{0}','{1}','{2}','{3}','{4}')", PKAgencyCodename,
                    PKClearenceLevel, EventName, TypeOfEvent, EstimatedEventDate.ToString("yyyy-M-dd")), con).ExecuteNonQuery();

            foreach (var a in AgentsInvolved)
                new SqlCommand(string.Format("INSERT INTO CIATestEvent_AgentLinkTable VALUES ('{0}','{1}',{2})",
                    PKAgencyCodename, PKClearenceLevel, a.PKAgentID),con).ExecuteNonQuery();

            foreach (CIATestReport report in Reports)
                report.CommitToDatabase(con);

        }

        public static bool IsExactMatchToDatabase(CIATestEvent[] events, SqlConnectionStringBuilder builder)
        {
            using (SqlConnection con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();

                DataTable dt = new DataTable();
                new SqlDataAdapter(new SqlCommand("Select * from CIATestEvent order by PKAgencyCodename asc",con)).Fill(dt);

                var orderedEvents = events.OrderBy(e => e.PKAgencyCodename).ToArray();

                if(dt.Rows.Count != orderedEvents.Length)
                    throw new Exception("There were a different number of rows in the database to the events[] array");

                for (int i = 0; i < orderedEvents.Length; i++ )
                    foreach (PropertyInfo p in typeof(CIATestEvent).GetProperties())
                    {
                        object o1 = p.GetValue(orderedEvents[i]);
                        object o2 = dt.Rows[i][p.Name];
                        
                        if (!AreTheSame(o1,o2))
                            return false;
                    }
                    
            }

             return true;
        }

        private static bool AreTheSame(object o1, object o2)
        {
            if (o1 is string && Equals(o1, ""))
                o1 = null;
            if (o2 is string && Equals(o2, ""))
                o2 = null;


            if(o1 == null)
                return o2 == null;

            if (o2 == null)
                return false;

            return o1.ToString().Equals(o2.ToString());
        }
    }
}