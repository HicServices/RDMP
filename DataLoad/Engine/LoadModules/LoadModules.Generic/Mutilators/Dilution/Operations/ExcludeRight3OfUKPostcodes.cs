using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace LoadModules.Generic.Mutilators.Dilution.Operations
{
    /// <summary>
    /// Dilutes data in the ColumnToDilute which is expected to contain postcodes by replacing stripping the last 3 digits such that DD3 7LX becomes DD3.
    /// See TestExcludeRight3OfUKPostcodes for expected inputs/outputs.
    /// </summary>
    public class ExcludeRight3OfUKPostcodes: DilutionOperation
    {
        public ExcludeRight3OfUKPostcodes() : 
            base(new DatabaseTypeRequest(typeof(string),4))
        {
        }

        public override void Check(ICheckNotifier notifier)
        {
            base.Check(notifier);

            if(!ColumnToDilute.SqlDataType.ToLower().Contains("char"))
                notifier.OnCheckPerformed(new CheckEventArgs("IPreLoadDiscardedColumn " + ColumnToDilute + " is of datatype " + ColumnToDilute.SqlDataType + " which is incompatible with this dilution operation (it must be char/varchar)", CheckResult.Fail));
        }
        
        public override string GetMutilationSql(INameDatabasesAndTablesDuringLoads namer)
        {
            return 

string.Format(@"

IF OBJECT_ID('dbo.RemoveDodgyCharacters') IS NOT NULL
  DROP FUNCTION RemoveDodgyCharacters
GO

Create Function [dbo].[RemoveDodgyCharacters](@Temp VarChar(max))
Returns VarChar(max)
AS
Begin

    Declare @KeepValues as varchar(50)
    Set @KeepValues = '%[^A-Za-z0-9]%'
    While PatIndex(@KeepValues, @Temp) > 0
        Set @Temp = Stuff(@Temp, PatIndex(@KeepValues, @Temp), 1, '')

    Return @Temp
End
GO

IF OBJECT_ID('dbo.Left4OfPostcodes') IS NOT NULL
  DROP FUNCTION Left4OfPostcodes
GO

CREATE FUNCTION Left4OfPostcodes
(
	-- Add the parameters for the function here
	@str varchar(max)
)
RETURNS varchar(4)
AS
BEGIN

    --Pass through nulls
    if @str IS NULL
        RETURN @str

    --Start by stripping out all dodgy characters (see method above)
    DECLARE @hackedStr varchar(max)

    set @hackedStr = dbo.RemoveDodgyCharacters(@str)

    --If the result is less than 3 characters, return the original string
    if LEN(@hackedStr) < 3
        RETURN @str
    
    --Part we are about to discard
    DECLARE @discardedBit varchar(3)
    SET @discardedBit = RIGHT(@hackedStr,3)

    --http://www.mrs.org.uk/pdf/postcodeformat.pdf
    --PO1 3AX
    
    --Have we identified the 3AX bit correctly?
    if PATINDEX('[0-9][A-Za-z][A-Za-z]' ,UPPER(@discardedBit)) = 1 --Yes
        RETURN SUBSTRING(@hackedStr,1,LEN(@hackedStr)-3) --Return the hacked string (no dodgy characters) minus the validated suffix
    
    --Suffix is missing or malformed but there is 5 or more characters so we aren't looking at 'DD3' we are looking at 'DD3 5L5' where the final digit should be a char but is an int by mistype
    if LEN(@hackedStr) > 4 AND  LEN(@hackedStr) < 8
        RETURN SUBSTRING(@hackedStr,1,LEN(@hackedStr)-3)

    RETURN @hackedStr --Else just return the hacked String (at least we did them the favour of removing dodgy characters and the varchar(4) means we probably fulfilled a contract reasonably anyway
END
GO

UPDATE {0} SET {1}=dbo.Left4OfPostcodes({1})
GO", ColumnToDilute.TableInfo.GetRuntimeName(LoadStage.AdjustStaging, namer), ColumnToDilute.GetRuntimeName());
        }
    }
}
