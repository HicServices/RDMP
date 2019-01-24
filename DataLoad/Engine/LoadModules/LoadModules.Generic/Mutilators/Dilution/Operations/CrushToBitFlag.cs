using System;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using FAnsi.Discovery.TypeTranslation;

namespace LoadModules.Generic.Mutilators.Dilution.Operations
{
    /// <summary>
    ///  Dilutes data in the ColumnToDilute by replacing all non null values with 1 and all null values with 0 then alters the column type to bit
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
            
            return 
String.Format(
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
}