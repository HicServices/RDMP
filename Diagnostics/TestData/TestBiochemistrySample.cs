// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CsvHelper;
using CsvHelper.Configuration;
using LoadModules.Generic.DataFlowSources;

namespace Diagnostics.TestData
{
    public class TestBiochemistrySample
    {
        /// <summary>
        /// every row in data table has a weigth (the number of records in our bichemistry with this sample type, this dictionary lets you input
        /// a record number 0-maxWeight and be returned an appropriate row from the table based on its weighting
        /// </summary>
        private static Dictionary<int, int> weightToRow;
        private static int maxWeight = -1;
        private static DataTable lookupTable;

        static TestBiochemistrySample()
        {
            string toFind = typeof(TestBiochemistrySample).Namespace + ".LabTestCodes.csv";
            var lookup = typeof(TestBiochemistrySample).Assembly.GetManifestResourceStream(toFind);

            if (lookup == null)
                throw new Exception("Could not find embedded resource file " + toFind);
          
            CsvReader r = new CsvReader(new StreamReader(lookup),new Configuration(){Delimiter =","});
            
            lookupTable = new DataTable();

            r.Read();
            r.ReadHeader();

            foreach (string header in r.Context.HeaderRecord)
                lookupTable.Columns.Add(header);
            
            r.Read();

            do
            {
                lookupTable.Rows.Add(r.Context.Record);
            } while (r.Read());
             
            weightToRow = new Dictionary<int, int>();

            int currentWeight = 0;
            for (int i = 0; i < lookupTable.Rows.Count; i++)
            {
                currentWeight += int.Parse(lookupTable.Rows[i]["frequency"].ToString());
                weightToRow.Add(currentWeight, i);
            }

            maxWeight = currentWeight;
        }


        public TestBiochemistrySample(Random r)
        {
            //get a random row from the lookup table - based on its representation within our biochemistry dataset
            DataRow row = GetRandomRowUsingWeight(r);

            Test_code = row["Test_code"].ToString();
            Sample_type = row["Sample_type"].ToString();
            
            double min;
            double max;

            bool hasMin = double.TryParse(row["minResult"].ToString(),out min);
            bool hasMax = double.TryParse(row["maxResult"].ToString(),out max);

            if(hasMin && hasMax)
                Result = ((r.NextDouble() * (max - min)) + min).ToString("#.##");
            else
                Result = "NULL";

            Units = row["Units"].ToString();
            ReadCodeValue = row["Read_code"].ToString();
            ReadCodeDescription = row["Description"].ToString();

        }

        private DataRow GetRandomRowUsingWeight(Random r)
        {
            int weightToGet = r.Next(maxWeight);

            //get the first key with a cumulative frequency above the one you are trying to get
            int row =  weightToRow.First(kvp => kvp.Key > weightToGet).Value;
            
            return lookupTable.Rows[row];
        }


        public string Sample_type;
        public string Test_code;
        public string Result;
        public string Units;
        public string ReadCodeValue;
        public string ReadCodeDescription;
    }
}