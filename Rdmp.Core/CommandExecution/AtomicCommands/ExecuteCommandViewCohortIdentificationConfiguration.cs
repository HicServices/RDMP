// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataViewing;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewCohortIdentificationConfiguration : BasicCommandExecution, IAtomicCommand
    {
        private readonly CohortIdentificationConfiguration _cic;
        private readonly bool _useCache;

        public ExecuteCommandViewCohortIdentificationConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic, bool useCache = true) : base(activator)
        {
            this._cic = cic;
            this._useCache = useCache;
        }

        public override void Execute()
        {
            base.Execute();

            BasicActivator.ShowData(
                new ViewCohortIdentificationConfigurationSqlCollection(_cic)
                {
                    UseQueryCache = _useCache
                });
        }
    }
}