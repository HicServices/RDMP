// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using FAnsi.Discovery.TypeTranslation;
using LoadModules.Generic.Mutilators.Dilution.Exceptions;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Mutilators.Dilution.Operations
{
    /// <summary>
    /// See IDilutionOperation
    /// </summary>
    public abstract class DilutionOperation : IPluginDilutionOperation
    {
        public DatabaseTypeRequest ExpectedDestinationType { get; private set; }

        protected DilutionOperation(DatabaseTypeRequest expectedDestinationType)
        {
            ExpectedDestinationType = expectedDestinationType;
        }


        public IPreLoadDiscardedColumn ColumnToDilute { set; protected get; }

        public virtual void Check(ICheckNotifier notifier)
        {
            if(ColumnToDilute == null)
                throw new DilutionColumnNotSetException("ColumnToDilute has not been set yet, this is the column which will be diluted and is usually set by the DilutionOperationFactory but it is null");

            if (string.IsNullOrWhiteSpace(ColumnToDilute.SqlDataType))
                notifier.OnCheckPerformed(new CheckEventArgs("IPreLoadDiscardedColumn " + ColumnToDilute + " is of unknown datatype", CheckResult.Fail));
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public abstract string GetMutilationSql(INameDatabasesAndTablesDuringLoads namer);
        
    }
}
