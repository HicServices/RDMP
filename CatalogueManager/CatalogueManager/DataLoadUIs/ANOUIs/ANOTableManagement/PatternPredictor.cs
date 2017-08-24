using System.Data.SqlClient;
using CatalogueLibrary.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.DataLoadUIs.ANOUIs.ANOTableManagement
{
    internal class PatternPredictor
    {
        private readonly ColumnInfo _columnInfo;

        #region horrible SQL code
        private const string SqlToCountLetters =
            @"
SELECT  MAX(LEN(thing)), MAX(
LEN(thing) - 
LEN(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE(thing
, 'A', '')
, 'B', '')
, 'C', '')
, 'D', '')
, 'E', '')
, 'F', '')
, 'G', '')
, 'H', '')
, 'I', '')
, 'J', '')
, 'K', '')
, 'L', '')
, 'M', '')
, 'N', '')
, 'O', '')
, 'P', '')
, 'Q', '')
, 'R', '')
, 'S', '')
, 'T', '')
, 'U', '')
, 'V', '')
, 'Q', '')
, 'X', '')
, 'Y', '')
, 'Z', '')
, 'a', '')
, 'b', '')
, 'c', '')
, 'd', '')
, 'e', '')
, 'f', '')
, 'g', '')
, 'h', '')
, 'i', '')
, 'j', '')
, 'k', '')
, 'l', '')
, 'm', '')
, 'n', '')
, 'o', '')
, 'p', '')
, 'q', '')
, 'r', '')
, 's', '')
, 't', '')
, 'u', '')
, 'v', '')
, 'w', '')
, 'x', '')
, 'y', '')
, 'z', '')))
FROM 
(select top 1000 FIELDTOEVALUATE as thing from TABLETOEVALUATE  order by newID()) bob";
        #endregion

        TableInfo _parent;

        public PatternPredictor(ColumnInfo columnInfo)
        {
            _columnInfo = columnInfo;
            _parent = columnInfo.TableInfo;
        }

        public string GetPattern(int timeoutInMilliseconds)
        {

            var server = DataAccessPortal.GetInstance().ExpectServer(_parent, DataAccessContext.InternalDataProcessing);

            using(var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand(
                    SqlToCountLetters
                        .Replace("FIELDTOEVALUATE",_columnInfo.GetRuntimeName())
                        .Replace("TABLETOEVALUATE", _parent.Name)
                    ,con);

                cmd.CommandTimeout = timeoutInMilliseconds;
                var reader = cmd.ExecuteReader();

                reader.Read();

                int longestString = int.Parse(reader[0].ToString());
                int largestNumberOfCharactersSpotted = int.Parse(reader[1].ToString());

                string resultPattern = "";

                for (int i = 0; i < largestNumberOfCharactersSpotted; i++)
                    resultPattern += 'Z';

                for (int i = 0; i < longestString - largestNumberOfCharactersSpotted; i++)
                    resultPattern += '9';

                //double up on the first character type (ask chris hall about this)
                resultPattern = resultPattern.ToCharArray()[0] + resultPattern;

                return resultPattern;
            }
        }
    }
}