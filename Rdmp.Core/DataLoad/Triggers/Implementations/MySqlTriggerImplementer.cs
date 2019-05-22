// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.DataLoad.Triggers.Implementations
{
    internal class MySqlTriggerImplementer:TriggerImplementer
    {
        public MySqlTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true) : base(table, createDataLoadRunIDAlso)
        {
        }

        public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
        {
            problemsDroppingTrigger = "";
            thingsThatWorkedDroppingTrigger = "";

            try
            {
                using (var con = _server.GetConnection())
                {
                    con.Open();

                    var cmd = _server.GetCommand("DROP TRIGGER " + GetTriggerName(), con);
                    cmd.ExecuteNonQuery();

                    thingsThatWorkedDroppingTrigger = "Droppped trigger " + GetTriggerName();
                }
            }
            catch (Exception exception)
            {
                //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
                problemsDroppingTrigger += "Failed to drop Trigger:" + exception.Message + Environment.NewLine; ;
            }
        }

        public override string CreateTrigger(ICheckNotifier notifier, int timeout = 30)
        {
            string creationSql = base.CreateTrigger(notifier, timeout);

            var sql = string.Format(@"CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW
{2};", 
       GetTriggerName(),
       _table.GetFullyQualifiedName(),
       CreateTriggerBody());

            using (var con = _server.GetConnection())
            {
                con.Open();

                var cmd = _server.GetCommand(sql, con);
                cmd.ExecuteNonQuery();
            }

            return creationSql;
        }

        protected virtual string CreateTriggerBody()
        {
            return string.Format(@"BEGIN
    INSERT INTO {0} SET {1},hic_validTo=now(),hic_userID=CURRENT_USER(),hic_status='U';
  END", _archiveTable.GetFullyQualifiedName(),
                string.Join(",", _columns.Select(c => c.GetRuntimeName() + "=OLD." + c.GetRuntimeName())));
        }
        
        public override TriggerStatus GetTriggerStatus()
        {
            return string.IsNullOrWhiteSpace(GetTriggerBody())? TriggerStatus.Missing : TriggerStatus.Enabled;
        }

        protected virtual string GetTriggerBody()
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                var cmd = _server.GetCommand(string.Format("show triggers like '{0}'", _table.GetRuntimeName()), con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    if (r["Trigger"].Equals(GetTriggerName()))
                        return (string) r["Statement"];
                }
            }

            return null;
        }

        protected virtual object GetTriggerName()
        {
            return _table.GetRuntimeName() + "_onupdate";
        }

        public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
        {
            if (!base.CheckUpdateTriggerIsEnabledAndHasExpectedBody())
                return false;

            var sqlThen = GetTriggerBody();
            var sqlNow = CreateTriggerBody();

            AssertTriggerBodiesAreEqual(sqlThen,sqlNow);
            
            return true;
        }

        protected virtual void AssertTriggerBodiesAreEqual(string sqlThen, string sqlNow)
        {
            if(!sqlNow.Equals(sqlThen))
                throw new ExpectedIdenticalStringsException("Sql body for trigger doesn't match expcted sql",sqlNow,sqlThen);
        }
    }
}
