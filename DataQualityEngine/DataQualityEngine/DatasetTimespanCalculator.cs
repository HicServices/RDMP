// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using DataQualityEngine.Data;

namespace DataQualityEngine
{
    /// <summary>
    /// Calculates the date range of data held in a dataset (Catalogue).  Optionally you can 'discardOutliers' this includes any dates in which there are
    /// 1000 times less records than the non zero average month.  For example if you have 3 records in 01/01/2090 then they would be discarded if you had
    ///  an average of 3000+ records per month (after ignoring months where there are no records).  
    /// 
    /// <para>IMPORTANT: You must have run the DQE on the dataset before this class can be used and the results are based on the last DQE run on the dataset not 
    /// the live table</para>
    /// </summary>
    public class DatasetTimespanCalculator : IDetermineDatasetTimespan
    {
        /// <inheritdoc/>
        public string GetHumanReadableTimepsanIfKnownOf(Catalogue catalogue,bool discardOutliers)
        {
            DataTable dt;

            try
            {
                var repo = new DQERepository(catalogue.CatalogueRepository);

                Evaluation mostRecentEvaluation = repo.GetMostRecentEvaluationFor(catalogue);

                if (mostRecentEvaluation == null)
                    return "Unknown";

                dt = PeriodicityState.GetPeriodicityForDataTableForEvaluation(mostRecentEvaluation, "ALL", false);
            }
            catch (Exception e)
            {
                return "Unknown:" + e.Message;
            }

            if (dt == null || dt.Rows.Count < 2)
                return "Unknown";

            int discardThreshold = discardOutliers? GetDiscardThreshold(dt):-1;

            string minMonth = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToInt32(dt.Rows[i]["CountOfRecords"]) > discardThreshold)
                {
                    minMonth = dt.Rows[i][1].ToString();
                    break;
                }
            }

            string maxMonth = null;
            for (int i = dt.Rows.Count-1; i >=0; i--)
            {
                if (Convert.ToInt32(dt.Rows[i]["CountOfRecords"]) > discardThreshold)
                {
                    maxMonth = dt.Rows[i][1].ToString();
                    break;
                }
            }

            if (maxMonth == null || minMonth == null)
                return "All Values Below Threshold";

            if (maxMonth == minMonth)
                return minMonth;


            return minMonth + " To " + maxMonth;

        }

        private int GetDiscardThreshold(DataTable dt)
        {
            int total = 0;
            int counted = 0;

            foreach (DataRow row in dt.Rows)
            {
                int currentValue = Convert.ToInt32(row["CountOfRecords"]);
                
                if(currentValue == 0)
                    continue;

                total += currentValue;
                counted++;
            }

            double nonZeroAverage = total/(double)counted;

            return (int)(nonZeroAverage/1000);
        }
    }
}
