// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Reflection;
using System.Text;
using MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDescribe:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable[] _toDescribe;

        public ExecuteCommandDescribe(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable[] toDescribe):base(activator)
        {
            _toDescribe = toDescribe;
        }

        public override void Execute()
        {
            base.Execute();

            var sb = new StringBuilder();

            foreach (IMapsDirectlyToDatabaseTable o in _toDescribe)
            {
                BuildDescribe(o,sb);
            }

            if(sb.Length > 0)
                Show(sb.ToString());
        }

        public static string Describe(IMapsDirectlyToDatabaseTable o)
        {
            var sb = new StringBuilder();
            BuildDescribe(o,sb);
            return sb.ToString();
        }

        private static void BuildDescribe(IMapsDirectlyToDatabaseTable o,StringBuilder sb)
        {
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                // don't describe helper properties
                if(p.GetCustomAttributes(typeof(NoMappingToDatabase)).Any())
                {
                    continue;
                }

                sb.Append(p.Name);
                sb.Append(":");
                sb.AppendLine(p.GetValue(o)?.ToString() ?? "NULL");
            }

            sb.AppendLine("-----------------------------------------");
        }
    }
}
