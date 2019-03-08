using System;
using System.IO;
using System.Text;

namespace Diagnostics.TestData.Exercises
{
    /// <summary>
    /// Test data based on the Scottish Vascular Labs CARSCAN database table
    /// </summary>
    public class CarotidArteryScanReportExerciseTestData : ExerciseTestDataGenerator
    {
        private Random r = new Random();
        private int id = 0;

        protected override object[] GenerateTestDataRow(TestPerson p)
        {
            object[] results = new object[68];

/*            TestBiochemistrySample randomSample = new TestBiochemistrySample(r);

            results[0] = p.CHI;
            results[1] = BulkTestsData.GetRandomLetter(true, r);
            results[2] = p.GetRandomDateDuringLifetime(r);
            results[3] = randomSample.Sample_type;
            results[4] = randomSample.Test_code;
            results[5] = randomSample.Result;
            results[6] = GetRandomLabNumber();
            results[7] = randomSample.Units;
            results[8] = randomSample.ReadCodeValue;
            results[9] = randomSample.ReadCodeDescription;*/

            var appointment = p.GetRandomAppointment(r);
            
            results[0] = appointment.Identifier; //RECORD_NUMBER
            results[1] = 0; //R_CC_STEN_A
            results[2] = 0; //R_CC_STEN_B
            results[3] = 0; //R_CC_STEN_C
            results[4] = 0; //R_CC_STEN_D
            results[5] = 0; //R_CC_STEN_S
            results[6] = 0; //L_IC_STEN_A
            results[7] = 0; //L_IC_STEN_B
            results[8] = 0; //L_IC_STEN_C
            results[9] = 0; //L_IC_STEN_D
            results[10] = 0; //L_IC_STEN_S
            results[11] = 0; //R_IC_STEN_A
            results[12] = 0; //R_IC_STEN_B
            results[13] = 0; //R_IC_STEN_C
            results[14] = 0; //R_IC_STEN_D
            results[15] = 0; //R_IC_STEN_S
            results[16] = Concat(r,0,3,()=>GetRandomSentence(r),Environment.NewLine + Environment.NewLine); //COMMENT
            results[17] = GetRandomSentence(r); ; //REPORT
            results[18] = id++; //id
            results[19] = p.CHI; //PatientID
            results[20] = 0; //SUMMARY
            results[21] = r.Next(0,99999); //LAST_AUTH_BY
            results[22] = TestPerson.GetRandomDateAfter(appointment.StartDate,r); //LAST_AUTH_DT
            results[23] = 0; //CV_FILE
            results[24] = 0; //L_CC_STEN_S
            results[25] = 0; //CV_DT
            results[26] = 0; //PROV_REPT_DT
            results[27] = 0; //FILE_COPY_DT
            results[28] = 0; //LAST_UPDATED_DT
            results[29] = 0; //AUTHORISED_DT
            results[30] = r.Next(0,3); //REPT_STATUS
            results[31] = GetGaussian(0,3); //STUDIES
            results[32] = 0; //FINAL_REPT_DT
            results[33] = 0; //hic_dataLoadRunID
            results[34] = 0; //L_CC_STEN_D
            results[35] = 0; //L_CC_STEN_B
            results[36] = 0; //APPT_ID
            results[37] = appointment.StartDate; //DATE
            results[38] = p.CHI; //CHINO
            results[39] = GetGaussian(0,5); //L_BD_RATIO
            results[40] = GetGaussian(0,3); //L_AC_RATIO
            results[41] = GetGaussian(0,10); //R_BD_RATIO
            results[42] = GetGaussian(0,5); //R_AC_RATIO
            results[43] = Math.Max(1, GetGaussianInt(-5, 9)); //L_CC_STENOSIS   (lots of 1's some non ones
            results[44] = GetGaussian(0,2); //L_CC_PEAK_SYS
            results[45] = GetGaussian(0,0.09); //L_GetGaussian(0,2);
            results[46] = GetGaussianInt(1,8); //L_IC_STENOSIS   
            results[47] = GetGaussian(0,4); //L_IC_PEAK_SYS
            results[48] = GetGaussian(0,4); //L_IC_END_DIA
            results[49] = Math.Max(1, GetGaussianInt(0, 9)); //L_EC_STENOSIS
            results[50] = Math.Max(1, GetGaussianInt(0, 9)); ; //L_PLAQUE
            results[51] = Math.Min(GetGaussianInt(1, 20), 8); //L_SYMPTOMS
            results[52] = 0; //L_BRUIT
            results[53] = 0; //L_CC_STEN_A
            results[54] = 0; //ON_STEN_STUDY
            results[55] = Math.Max(1, GetGaussianInt(-5, 4)); ; //R_VERT_ARTERY
            results[56] = 0; //R_BRUIT
            results[57] = Math.Min(GetGaussianInt(1, 20), 8); //R_SYMPTOMS
            results[58] = Math.Max(1, GetGaussianInt(0, 9));//R_PLAQUE
            results[59] = 0; //L_CC_STEN_C
            results[60] = Math.Max(1, GetGaussianInt(-5, 9)); ; //R_EC_STENOSIS
            results[61] = 0; //R_IC_PEAK_SYS
            results[62] = 0; //R_IC_STENOSIS
            results[63] = GetGaussian(0,0.2); //R_CC_END_DIA
            results[64] = GetGaussian(0, 2); ; //R_CC_PEAK_SYS
            results[65] = Math.Max(1, GetGaussianInt(-5, 9)); //R_CC_STENOSIS
            results[66] = Math.Max(1, GetGaussianInt(-5, 4)); //L_VERT_ARTERY
            results[67] = GetGaussian(0, 2); //R_IC_END_DIA

            return results;
        }

        


        protected override void WriteHeaders(StreamWriter sw)
        {

            string[] h =
            {
                "RECORD_NUMBER",    //0
                "R_CC_STEN_A",      //1
                "R_CC_STEN_B",      //2
                "R_CC_STEN_C",      //3
                "R_CC_STEN_D",      //4
                "R_CC_STEN_S",      //5
                "L_IC_STEN_A",      //6
                "L_IC_STEN_B",      //7
                "L_IC_STEN_C",      //8
                "L_IC_STEN_D",      //9
                "L_IC_STEN_S",      //10
                "R_IC_STEN_A",      //11
                "R_IC_STEN_B",      //12
                "R_IC_STEN_C",      //13
                "R_IC_STEN_D",      //14
                "R_IC_STEN_S",      //15
                "COMMENT",          //16
                "REPORT",           //17
                "id",               //18
                "PatientID",        //19
                "SUMMARY",          //20
                "LAST_AUTH_BY",     //21
                "LAST_AUTH_DT",     //22
                "CV_FILE",          //23
                "L_CC_STEN_S",      //24
                "CV_DT",            //25
                "PROV_REPT_DT",     //26
                "FILE_COPY_DT",     //27
                "LAST_UPDATED_DT",  //28
                "AUTHORISED_DT",    //29
                "REPT_STATUS",      //30
                "STUDIES",          //31
                "FINAL_REPT_DT",    //32
                "hic_dataLoadRunID",//33
                "L_CC_STEN_D",      //34
                "L_CC_STEN_B",      //35
                "APPT_ID",          //36
                "DATE",             //37
                "CHINO",            //38
                "L_BD_RATIO",       //39
                "L_AC_RATIO",       //40
                "R_BD_RATIO",       //41
                "R_AC_RATIO",       //42
                "L_CC_STENOSIS",    //43
                "L_CC_PEAK_SYS",    //44
                "L_CC_END_DIA",     //45
                "L_IC_STENOSIS",    //46
                "L_IC_PEAK_SYS",    //47
                "L_IC_END_DIA",     //48
                "L_EC_STENOSIS",    //49
                "L_PLAQUE",         //50
                "L_SYMPTOMS",       //51
                "L_BRUIT",          //52
                "L_CC_STEN_A",      //53
                "ON_STEN_STUDY",    //54
                "R_VERT_ARTERY",    //55
                "R_BRUIT",          //56
                "R_SYMPTOMS",       //57
                "R_PLAQUE",         //58
                "L_CC_STEN_C",      //59
                "R_EC_STENOSIS",    //60
                "R_IC_PEAK_SYS",    //61
                "R_IC_STENOSIS",    //62
                "R_CC_END_DIA",     //63
                "R_CC_PEAK_SYS",    //64
                "R_CC_STENOSIS",    //65
                "L_VERT_ARTERY",    //66
                "R_IC_END_DIA"      //67
            };

            sw.WriteLine(string.Join(",", h));
        }                             
    }                                 
}                                     
