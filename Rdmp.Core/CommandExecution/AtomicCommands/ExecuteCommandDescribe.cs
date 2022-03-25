// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDescribe:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable[] _toDescribe;
        private object _randomThing;

        public ExecuteCommandDescribe(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable[] toDescribe):base(activator)
        {
            _toDescribe = toDescribe;
        }

        public ExecuteCommandDescribe(IBasicActivateItems basicActivator, object randomThing) : base(basicActivator)
        {
            _randomThing = randomThing;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            if (_randomThing != null)
                return iconProvider.GetImage(_randomThing);

            return base.GetImage(iconProvider);
        }
        public override string GetCommandName()
        {
            if (_randomThing != null)
            {
                return _randomThing is Type t ? t.Name : _randomThing.ToString();
            }

            return base.GetCommandName();
        }
        public override string GetCommandHelp()
        {
            if(_randomThing != null)
            {
                string summary = BuildDescribe(_randomThing, out string title);

                return title + Environment.NewLine + summary;
            }

            return base.GetCommandHelp();
        }
        public override void Execute()
        {
            base.Execute();

            if(_randomThing != null)
            {
                var description = BuildDescribe(_randomThing,out string title);
                Show(title, description);
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (IMapsDirectlyToDatabaseTable o in _toDescribe)
                {
                    BuildDescribe(o, sb);
                }

                if (sb.Length > 0)
                    Show(sb.ToString());
            }

        }

        public static string Describe(IMapsDirectlyToDatabaseTable o)
        {
            var sb = new StringBuilder();
            BuildDescribe(o,sb);
            return sb.ToString();
        }

        private static void BuildDescribe(IMapsDirectlyToDatabaseTable o, StringBuilder sb)
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
        private string BuildDescribe(object o,out string title)
        {
            // if its a Type tell them about the Type
            if (o is Type t)
            {
                title = t.Name;
                var docs = BasicActivator.CommentStore.GetTypeDocumentationIfExists(t, true, true);
                return $"A {t.Name} will be available for reading by components when the pipeline is run.{Environment.NewLine}{Environment.NewLine}{docs}".Trim();
            }

            // its an actual object so give them a summary of it
            title = _randomThing.GetType().Name;
            return o is ICanBeSummarised s ? s.GetSummary(true, true) : o.ToString();
        }
    }
}
