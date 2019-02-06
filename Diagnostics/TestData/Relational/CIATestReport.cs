// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FAnsi.Discovery;

namespace Diagnostics.TestData.Relational
{
    public class CIATestReport
    {
        public int PKID;
        public string ReportText;
        public DateTime ReportDate;
        public string PKFKAgencyCodename;
        public CIATestClearenceLevel PKFKClearenceLevel;

        public CIATestInformant CIATestInformantSignatory1;
        public CIATestInformant CIATestInformantSignatory2;
        public CIATestInformant CIATestInformantSignatory3;

        public CIATestReport(Random r,CIATestEvent parent, List<CIATestReport> avoidDuplicationWith, List<CIATestInformant> allInformants)
        {
            int id = 1;

            while (avoidDuplicationWith.Any(report => report.PKID == id))
            {
                id = r.Next(100000);
            }

            PKID = id;
            ReportText = GetRandomText(r);
            
            PKFKAgencyCodename = parent.PKAgencyCodename;
            PKFKClearenceLevel = parent.PKClearenceLevel;

            //create the reports around the time of the parents estimated event date
            if (r.Next(2) == 1)
            {
                try
                {
                    ReportDate = parent.EstimatedEventDate.AddDays(r.Next(3));
                }
                catch (Exception)
                {
                    ReportDate = DateTime.MaxValue;
                }
            }
            else
                try
                {
                    ReportDate = parent.EstimatedEventDate.AddDays(-r.Next(3));
                }
                catch (Exception)
                {
                    ReportDate = DateTime.MinValue;
                }


            CIATestInformantSignatory1 = allInformants[r.Next(allInformants.Count)];

            //50% chance of having a second
            if(r.Next(2) ==0)
            {
                CIATestInformantSignatory2 = allInformants[r.Next(allInformants.Count)];

                //25% chance of having a third
                if (r.Next(2) == 0)
                    CIATestInformantSignatory3 = allInformants[r.Next(allInformants.Count)];
            }

        }

        private string GetRandomText(Random random)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < random.Next(100); i++)
                sb.Append(GetRandomWord(random));

            return sb.ToString();
        }

        private string GetRandomWord(Random random)
        {
            switch (random.Next(25))
            {
                case 0: return "Agent ";
                case 1: return "Walking ";
                case 2: return "Shot ";
                case 3: return "Sidewalk ";
                case 4: return "Aimed ";
                case 5: return "Overseas ";
                case 6: return "Nuclear ";
                case 7: return "Suitcase ";
                case 8: return "Suspect ";
                case 9: return "Nevermore ";
                case 10: return "Computer ";
                case 11: return "Surveillence ";
                case 12: return "Systemic ";
                case 13: return "Corruption ";
                case 14: return "Found ";
                case 15: return "Park ";
                case 16: return "Secret ";
                case 17: return "Telephone ";
                case 18: return "Mobile ";
                case 19: return "Operational ";
                case 20: return "Cantered ";
                case 21: return "Codename ";
                case 22: return "Seen ";
                case 23: return "Running ";
                case 24: return "Moscow ";
            }
        
            throw new ArgumentOutOfRangeException();
        }

        public void CommitToDatabase(DiscoveredDatabase database, DbConnection con)
        {
            database.Server.GetCommand(string.Format("INSERT INTO CIATestReport VALUES ({0},'{1}','{2}','{3}','{4}',{5},{6},{7})", PKID, ReportText, ReportDate.ToString("yyyy-MM-dd"), PKFKAgencyCodename, PKFKClearenceLevel,
                CIATestInformantSignatory1 == null ? "null" : CIATestInformantSignatory1.ID.ToString(),
                CIATestInformantSignatory2 == null ? "null" : CIATestInformantSignatory2.ID.ToString(),
                CIATestInformantSignatory3 == null ? "null" : CIATestInformantSignatory3.ID.ToString()), con).ExecuteNonQuery();
        }
    }
}