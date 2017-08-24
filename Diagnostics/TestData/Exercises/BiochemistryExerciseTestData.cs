using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary;
using ReusableLibraryCode.Progress;

namespace Diagnostics.TestData.Exercises
{
    public class BiochemistryExerciseTestData : ExerciseTestDataGenerator
    {
        private Random r = new Random();
        protected override object[] GenerateTestDataRow(TestPerson p)
        {
            object[] results = new object[10];

            TestBiochemistrySample randomSample = new TestBiochemistrySample(r);
            
            results[0] = p.CHI;
            results[1] = BulkTestsData.GetRandomLetter(true, r);
            results[2] = p.GetRandomDateDuringLifetime(r);
            results[3] = randomSample.Sample_type;
            results[4] = randomSample.Test_code;
            results[5] = randomSample.Result;
            results[6] = GetRandomLabNumber();
            results[7] = randomSample.Units;
            results[8] = randomSample.ReadCodeValue;
            results[9] = randomSample.ReadCodeDescription;

            return results;

        }

        protected override void WriteHeaders(StreamWriter sw)
        {
            string[] h =
            {
                "chi",                              //0
                "hb_extract",                       //1
                "Sample_date",                      //2
                "Sample_type",                      //3
                "Test_code",                        //4
                "Result",                           //5
                "Labnumber",                        //6
                "Units",                            //7
                "ReadCodeValue",                    //8
                "ReadCodeDescription"               //9
            };

            sw.WriteLine(string.Join(",",h));
        }

        private string GetRandomLabNumber()
        {
            if(r.Next(0,2)==0)
                return "CC" + r.Next(0, 1000000);

            return "BC" + r.Next(0, 1000000);
        }
    }
}
