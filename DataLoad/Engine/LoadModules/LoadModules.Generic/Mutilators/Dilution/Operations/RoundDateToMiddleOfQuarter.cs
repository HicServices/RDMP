using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using LoadModules.Generic.Mutilators.Dilution.Exceptions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace LoadModules.Generic.Mutilators.Dilution.Operations
{
    /// <summary>
    ///  Dilutes data in the ColumnToDilute by rounding all dates to the middle of the quarter they appear in (column type must be date and is not changed by
    /// this DilutionOperation). Data type of column must be date and will not be changed by this DilutionOperation.
    /// </summary>
    public class RoundDateToMiddleOfQuarter : DilutionOperation
    {
        public RoundDateToMiddleOfQuarter() :
            base(new DatabaseTypeRequest(typeof(DateTime)))
        {
        }

        public override void Check(ICheckNotifier notifier)
        {
            base.Check(notifier);

            //confirm type safety
            if (!ColumnToDilute.SqlDataType.ToLower().Contains("date"))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ColumnToDilute '" + ColumnToDilute.RuntimeColumnName +
                    "' has operation RoundDateToMiddleOfQuarter configured but it's datatype is " +
                    ColumnToDilute.SqlDataType, CheckResult.Fail, null));
        }
        
        public override string GetMutilationSql(INameDatabasesAndTablesDuringLoads namer)
        {
            return String.Format(@"IF OBJECT_ID('dbo.RoundDateToMiddleOfQuarter') IS NOT NULL
  DROP FUNCTION RoundDateToMiddleOfQuarter
GO

CREATE FUNCTION RoundDateToMiddleOfQuarter
(
	-- Add the parameters for the function here
	@DOB date
)
RETURNS date
AS
BEGIN
	-- Declare the return variable here
	DECLARE @anonDOB date

	-- Add the T-SQL statements to compute the return value here
      IF MONTH(@DOB) IN (1,2,3)
	    SET @anonDOB =   LEFT(@DOB, 4) + '0215'
      ELSE IF MONTH(@DOB) IN (4,5,6)
	    SET @anonDOB = LEFT(@DOB, 4) + '0515' 
      ELSE IF MONTH(@DOB) IN (7,8,9)
	    SET @anonDOB = LEFT(@DOB, 4) + '0815' 
	  ELSE IF MONTH(@DOB)IN (10,11,12)
	    SET @anonDOB = LEFT(@DOB, 4) + '1115' 
     ELSE SET @anonDOB = NULL
	
	-- Return the result of the function
	RETURN @anonDOB
END
GO

UPDATE {0} SET {1}=dbo.RoundDateToMiddleOfQuarter({1})
GO",
   ColumnToDilute.TableInfo.GetRuntimeName(LoadStage.AdjustStaging, namer), ColumnToDilute.GetRuntimeName());
        }
    }
}
