// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using TypeGuesser;

namespace Rdmp.Core.DataLoad.Modules.Mutilators.Dilution.Operations;

/// <summary>
///     Dilutes data in the ColumnToDilute by replacing all non null values with 1 and all null values with 0 then alters
///     the column type to bit
/// </summary>
/// <returns></returns>
public class CrushToBitFlag : DilutionOperation
{
    public CrushToBitFlag() :
        base(new DatabaseTypeRequest(typeof(bool)))
    {
    }

    public override string GetMutilationSql(INameDatabasesAndTablesDuringLoads namer)
    {
        return string.Format(
            @"
  ALTER TABLE {0} Add {1}_bit bit 
  GO

  UPDATE {0} SET {1}_bit = CASE WHEN {1} is null THEN 0 else 1 end
  GO

  ALTER TABLE {0} DROP column {1}
  GO

  EXEC sp_rename '{0}.{1}_bit', '{1}' , 'COLUMN'
  GO
", ColumnToDilute.TableInfo.GetRuntimeName(LoadStage.AdjustStaging, namer), ColumnToDilute.GetRuntimeName());
    }
}