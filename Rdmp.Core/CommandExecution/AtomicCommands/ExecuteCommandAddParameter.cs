// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Adds a new SqlParameter to an <see cref="ICollectSqlParameters"/>
    /// </summary>
    public class ExecuteCommandAddParameter : BasicCommandExecution, IAtomicCommand
    {
        private readonly ICollectSqlParameters _collector;
        private readonly string parameterName;

        public ExecuteCommandAddParameter(IBasicActivateItems activator, ICollectSqlParameters collector, string parameterName) : base(activator)
        {
            _collector = collector;
            this.parameterName = parameterName;
            UseTripleDotSuffix = true;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ParametersNode);
        }

        public override void Execute()
        {
            ParameterCollectionUIOptionsFactory factory = new ParameterCollectionUIOptionsFactory();
            var options = factory.Create(_collector, BasicActivator.CoreChildProvider);

            var n = parameterName;

            if (n == null)
            {
                // get user to type the name of the parameter
                if (BasicActivator.TypeText(new DialogArgs
                {
                    EntryLabel = "Name",
                    TaskDescription = "A name is required for the paramater.  It must start with '@' e.g. @myparameter.  Do not add spaces or start the name with a number.",
                    WindowTitle = "Add Paramater"
                }, 100, null, out string name,false))
                {
                    // user did type a name
                    n = name;
                }
                else
                {
                    // user cancelled typing the parameter name
                    return;
                }
            }

            options.CreateNewParameter(n);

            if (_collector is DatabaseEntity d)
            {
                Publish(d);
            }

        }
    }
}