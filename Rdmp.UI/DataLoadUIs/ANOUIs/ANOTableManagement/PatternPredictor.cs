// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.DataLoadUIs.ANOUIs.ANOTableManagement;

internal class PatternPredictor
{
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

    private readonly ColumnInfo _columnInfo;

    private readonly TableInfo _parent;

    public PatternPredictor(ColumnInfo columnInfo)
    {
        _columnInfo = columnInfo;
        _parent = columnInfo.TableInfo;
    }

    public string GetPattern(int timeoutInMilliseconds)
    {
        var server = DataAccessPortal.ExpectServer(_parent, DataAccessContext.InternalDataProcessing);

        using (var con = server.GetConnection())
        {
            con.Open();

            using (var cmd = server.GetCommand(
                       SqlToCountLetters
                           .Replace("FIELDTOEVALUATE", _columnInfo.GetRuntimeName())
                           .Replace("TABLETOEVALUATE", _parent.Name)
                       , con))
            {
                cmd.CommandTimeout = timeoutInMilliseconds;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    var longestString = int.Parse(reader[0].ToString());
                    var largestNumberOfCharactersSpotted = int.Parse(reader[1].ToString());

                    var resultPattern = "";

                    for (var i = 0; i < largestNumberOfCharactersSpotted; i++)
                        resultPattern += 'Z';

                    for (var i = 0; i < longestString - largestNumberOfCharactersSpotted; i++)
                        resultPattern += '9';

                    //double up on the first character type (ask chris hall about this)
                    resultPattern = resultPattern.ToCharArray()[0] + resultPattern;

                    return resultPattern;
                }
            }
        }
    }
}