// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using Rdmp.Core.Validation.Constraints;
using ReusableLibraryCode;

namespace Rdmp.Core.DataQualityEngine.Data
{
    /// <summary>
    /// Runtime class for DQE used to record the number of rows passing/failing validation/null overall.  This is calculated by validating every column in the row
    /// and selecting the worst validation failure Consequence (if any) for the row.
    /// 
    /// <para>These counts are incremented during the DQE evaluation process then finally saved into the PeriodicityState table in DQE database.</para>
    /// </summary>
   public class PeriodicityState
    {
        private int _countOfRecords;
        public int Year { get; private set; }
        public int Month { get; private set; }

        public int CountOfRecords
        {
            get { return _countOfRecords; }
            set
            {
                if(IsCommitted )
                    throw new NotSupportedException("Can only edit these values while the PeriodicityState is being computed in memory, this PeriodicityState came from the database and was committed long ago");
                _countOfRecords = value; 
            }
        }

        public string RowEvaluation { get; private set; }

        public bool IsCommitted { get; private set; }

        public PeriodicityState(int year, int month, Consequence? consequence )
        {
            IsCommitted = false;
            Year = year;
            Month = month;

            RowEvaluation = consequence == null ? "Correct" : consequence.ToString();

        }

       public static Dictionary<DateTime, ArchivalPeriodicityCount> GetPeriodicityCountsForEvaluation(Evaluation evaluation,bool discardOutliers)
       {
           var toReturn = new Dictionary<DateTime, ArchivalPeriodicityCount>();

           var calc = new DatasetTimespanCalculator();
           var result = calc.GetMachineReadableTimespanIfKnownOf(evaluation, discardOutliers);

           using (var con = evaluation.DQERepository.GetConnection())
           {
               var sql = 
               @"SELECT 
      [Year]
      ,[Month]
      ,RowEvaluation
      ,CountOfRecords
  FROM [PeriodicityState]
    where
  Evaluation_ID = " + evaluation.ID + " and PivotCategory = 'ALL' ORDER BY [Year],[Month]";

               using(var cmd = DatabaseCommandHelper.GetCommand(sql, con.Connection, con.Transaction))
               {
                   using (var r = cmd.ExecuteReader())
                   {
                       while (r.Read())
                       {
                           var date = new DateTime((int) r["Year"], (int) r["Month"], 1);

                           //discard outliers before start
                           if(discardOutliers && result.Item1.HasValue && date < result.Item1.Value)
                           {
                                continue;
                           }

                            //discard outliers after end
                            if (discardOutliers && result.Item2.HasValue && date > result.Item2.Value)
                            {
                                continue;
                            }

                            if (!toReturn.ContainsKey(date))
                               toReturn.Add(date,new ArchivalPeriodicityCount());

                           var toIncrement = toReturn[date];

                           /*
                            *
        Correct
        InvalidatesRow
        Missing
        Wrong
                            * */

                           switch ((string)r["RowEvaluation"])
                           {
                               case "Correct": 
                                   toIncrement.CountGood += (int) r["CountOfRecords"];
                                   toIncrement.Total += (int)r["CountOfRecords"];
                                   break;
                               case "InvalidatesRow": toIncrement.Total += (int)r["CountOfRecords"];
                                   break;
                               case "Missing": toIncrement.Total += (int)r["CountOfRecords"];
                                   break;
                               case "Wrong": toIncrement.Total += (int)r["CountOfRecords"];
                                   break;
                               default:throw new ArgumentOutOfRangeException("Unexpected RowEvaluation '" + r["RowEvaluation"] + "'");
                           }
                       }
                   }
               }
           }

           return toReturn;
       }
       
        public static DataTable GetPeriodicityForDataTableForEvaluation(Evaluation evaluation, string pivotCategoryValue, bool pivot)
        {
            using (var con = evaluation.DQERepository.GetConnection())
            {
                string sql = "";

                if (pivot)
                    sql = string.Format(PeriodicityPivotSql, evaluation.ID, pivotCategoryValue);
                else
                    sql = @"Select [Evaluation_ID]
      ,CAST([Year] as varchar(4)) + '-' + datename(month,dateadd(month, [Month] - 1, 0)) as YearMonth
      ,[CountOfRecords]
      ,[RowEvaluation] from PeriodicityState where Evaluation_ID=" + evaluation.ID + " AND PivotCategory = '" + pivotCategoryValue+"'";


                using(var cmd = DatabaseCommandHelper.GetCommand(sql, con.Connection, con.Transaction))
                    using (var da = DatabaseCommandHelper.GetDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if(pivot)
                        {
                            dt.Columns["Correct"].SetOrdinal(3);
                            dt.Columns["Wrong"].SetOrdinal(4);
                            dt.Columns["Missing"].SetOrdinal(5);
                            dt.Columns["InvalidatesRow"].SetOrdinal(6);
                        }

                        return dt;
                    }
            }
        }

       public void Commit(Evaluation evaluation, string pivotCategory)
        {
            using (var con = evaluation.DQERepository.GetConnection())
            {
                if (IsCommitted)
                    throw new NotSupportedException("PeriodicityState was already committed");

                string sql =
                    string.Format(
                        "INSERT INTO [dbo].[PeriodicityState]([Evaluation_ID],[Year],[Month],[CountOfRecords],[RowEvaluation],[PivotCategory])VALUES({0},{1},{2},{3},{4},{5})"
                        ,evaluation.ID
                        ,Year
                        ,Month
                        ,CountOfRecords
                        , "@RowEvaluation",
                        "@PivotCategory");

                using (var cmd = DatabaseCommandHelper.GetCommand(sql, con.Connection, con.Transaction))
                {
                    DatabaseCommandHelper.AddParameterWithValueToCommand("@RowEvaluation", cmd, RowEvaluation);
                    DatabaseCommandHelper.AddParameterWithValueToCommand("@PivotCategory", cmd, pivotCategory);
                    cmd.ExecuteNonQuery();
                }

                IsCommitted = true;
            }
        }

       private const string PeriodicityPivotSql = @"
--DYNAMICALLY FETCH COLUMN VALUES FOR USE IN PIVOT
DECLARE @Columns as VARCHAR(MAX)

--Get distinct values of the PIVOT Column if you have columns with values T and F and Z this will produce [T],[F],[Z] and you will end up with a pivot against these values
SELECT @Columns= ISNULL(@Columns + ',' + CHAR(13),'') 
       + QUOTENAME(RowEvaluation)
FROM (SELECT DISTINCT LTRIM(RTRIM([PeriodicityState].[RowEvaluation])) AS RowEvaluation FROM 
[PeriodicityState] WHERE [PeriodicityState].[RowEvaluation] IS NOT NULL and [PeriodicityState].[RowEvaluation] <> ''  AND 
(
--Evaluation ID is X
[PeriodicityState].[Evaluation_ID] = {0}
AND
[PeriodicityState].[PivotCategory] = '{1}'
)
) as RockOnBuddy

--DYNAMIC PIVOT
declare @Query varchar(MAX)

SET @Query = '

select *
from
(

SELECT 
CAST([Year] as varchar(4)) + ''-'' + datename(month,dateadd(month, [Month] - 1, 0)) as YearMonth,
[Year],
[Month],
[PeriodicityState].[RowEvaluation],
sum(CountOfRecords) AS MyCount
FROM 
[PeriodicityState]
WHERE
(
--Evaluation ID is X
[PeriodicityState].[Evaluation_ID] = {0} AND [PeriodicityState].[PivotCategory] = ''{1}''
)
group by 
CAST([Year] as varchar(4)) + ''-'' +  datename(month,dateadd(month, [Month] - 1, 0)),
[Year],
[Month],
[PeriodicityState].[RowEvaluation]) s

PIVOT
(
    sum(MyCount)
    for RowEvaluation in ('+@Columns+') --The dynamic Column list we just fetched at top of query
) piv
order by 
[Year],
[Month]
'

EXECUTE(@Query)";
    }
}
