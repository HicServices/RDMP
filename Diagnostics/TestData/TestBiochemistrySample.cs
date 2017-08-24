using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

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
          
             
            CsvReader r = new CsvReader(new StreamReader(lookup),new CsvConfiguration(){Delimiter =","});
            
            
            lookupTable = new DataTable();

            r.Read();
            foreach (string header in r.FieldHeaders)
                lookupTable.Columns.Add(header);

            do
            {
                lookupTable.Rows.Add(r.CurrentRecord);
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