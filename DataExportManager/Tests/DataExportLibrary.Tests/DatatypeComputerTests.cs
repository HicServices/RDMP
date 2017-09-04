using System;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace DataExportLibrary.Tests
{
    public class DatatypeComputerTests
    {
        private MicrosoftSQLTypeTranslater _translater;

        public DatatypeComputerTests()
        {
            _translater = new MicrosoftSQLTypeTranslater();

        }
        [Test]
        public void TestDatatypeComputer_decimal()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("1.5");
            t.AdjustToCompensateForValue("299.99");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(decimal),t.CurrentEstimate);
        }

        [Test]
        public void TestDatatypeComputer_Int()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("15");
            t.AdjustToCompensateForValue("299");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(int),t.CurrentEstimate);
        }

        [Test]
        public void TestDatatypeComputer_IntAnddecimal_MustUsedecimal()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("15");
            t.AdjustToCompensateForValue("29.9");
            t.AdjustToCompensateForValue("200");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(t.CurrentEstimate, typeof(decimal));
            Assert.AreEqual("decimal(4,1)", t.GetSqlDBType(_translater));
        }

        [Test]
        public void TestDatatypeComputer_DateTime()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("01/01/2001");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(t.CurrentEstimate, typeof(DateTime));
            Assert.AreEqual("datetime2", t.GetSqlDBType(_translater));
        }

        [Test]
        public void TestDatatypeComputer_DateTime_English()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("23/01/2001");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(t.CurrentEstimate, typeof(DateTime));
            Assert.AreEqual("datetime2", t.GetSqlDBType(_translater));
        }

        [Test]
        public void TestDatatypeComputer_DateTime_EnglishWithTime()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("23/01/2001 11:10");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(t.CurrentEstimate, typeof(DateTime));
            Assert.AreEqual("datetime2", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_DateTime_EnglishWithTimeAndAM()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("23/01/2001 11:10AM");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(t.CurrentEstimate, typeof(DateTime));
            Assert.AreEqual("datetime2", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_PreeceedingZeroes()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("1.5");
            t.AdjustToCompensateForValue("00299.99");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(string), t.CurrentEstimate);
        }
        [Test]
        public void TestDatatypeComputer_Negatives()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("-1");
            t.AdjustToCompensateForValue("-99.99");

            Assert.AreEqual(t.CurrentEstimate, typeof(decimal));
            Assert.AreEqual("decimal(4,2)", t.GetSqlDBType(_translater));
        }


        [Test]
        public void TestDatatypeComputer_Doubles()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(299.99);
            
            Assert.AreEqual(typeof(double), t.CurrentEstimate);

            Assert.AreEqual(2, t.numbersAfterDecimalPlace);
            Assert.AreEqual(3, t.numbersBeforeDecimalPlace);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestDatatypeComputer_Bool(bool sendStringEquiv)
        {
            DataTypeComputer t = new DataTypeComputer();

            if (sendStringEquiv)
                t.AdjustToCompensateForValue("True");
            else
                t.AdjustToCompensateForValue(true);
            
            if (sendStringEquiv)
                t.AdjustToCompensateForValue("False");
            else
                t.AdjustToCompensateForValue(false);
            
            Assert.AreEqual(typeof(bool), t.CurrentEstimate);

            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(typeof(bool), t.CurrentEstimate);

            Assert.AreEqual(-1, t.numbersAfterDecimalPlace);
            Assert.AreEqual(-1, t.numbersBeforeDecimalPlace);
        }

        [Test]
        public void TestDatatypeComputer_MixedIntTypes()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue((Int16)5);
            var ex = Assert.Throws<Exception>(()=>t.AdjustToCompensateForValue((Int32)1000));

            Assert.IsTrue(ex.Message.Contains("We were adjusting to compensate for object 1000 which is of Type System.Int32 , we were previously passed a System.Int16 type, is your column of mixed type? this is unacceptable"));
        }
        [Test]
        public void TestDatatypeComputer_Int16s()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue((Int16)5);
            t.AdjustToCompensateForValue((Int16)10);
            t.AdjustToCompensateForValue((Int16)15);
            t.AdjustToCompensateForValue((Int16)30);
            t.AdjustToCompensateForValue((Int16)200);

            Assert.AreEqual(typeof(Int16), t.CurrentEstimate);

            Assert.AreEqual(3, t.numbersBeforeDecimalPlace);
            Assert.AreEqual(-1, t.numbersAfterDecimalPlace);
            

        }
        [Test]
        public void TestDatatypeComputer_Byte()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(new byte[5]);

            Assert.AreEqual(typeof(byte[]), t.CurrentEstimate);

            Assert.AreEqual(-1, t.numbersAfterDecimalPlace);
            Assert.AreEqual(-1, t.numbersBeforeDecimalPlace);
        }

        [Test]
        public void TestDatatypeComputer_NumberOfDecimalPlaces()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("111111111.11111111111115");
            
            Assert.AreEqual(typeof(decimal), t.CurrentEstimate);
            Assert.AreEqual(9, t.numbersBeforeDecimalPlace);
            Assert.AreEqual(14, t.numbersAfterDecimalPlace);
        }


        [Test]
        public void TestDatatypeComputer_FallbackOntoVarcharFromFloat()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("15.5");
            t.AdjustToCompensateForValue("F");

            Assert.AreEqual(typeof(string), t.CurrentEstimate);
            Assert.AreEqual("varchar(4)", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_Time()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("12:30:00");

            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);
            Assert.AreEqual("time", t.GetSqlDBType(_translater));
        }

        [Test]
        public void TestDatatypeComputer_TimeNoSeconds()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("12:01");

            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);
            Assert.AreEqual("time", t.GetSqlDBType(_translater));
        }

        [Test]
        public void TestDatatypeComputer_TimeWithPM()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("1:01PM");

            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);
            Assert.AreEqual("time", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_24Hour()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("23:01");

            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);
            Assert.AreEqual("time", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_Midnight()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("00:00");

            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);
            Assert.AreEqual("time", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_TimeObject()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(new TimeSpan(10,1,1));

            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);
            Assert.AreEqual("time", t.GetSqlDBType(_translater));
        }
        [Test]
        public void TestDatatypeComputer_MixedDateAndTime_FallbackToString()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("09:01");
            Assert.AreEqual(typeof(TimeSpan), t.CurrentEstimate);

            t.AdjustToCompensateForValue("2001-12-29 23:01");
            Assert.AreEqual(typeof(string), t.CurrentEstimate);
            Assert.AreEqual("varchar(16)", t.GetSqlDBType(_translater));
        }

        [TestCase("1-1000")]
        public void TestDatatypeComputer_ValidDateStrings(string wierdDateString)
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(wierdDateString);
            Assert.AreEqual(typeof(DateTime), t.CurrentEstimate);
        }

        [TestCase("1234",typeof(int),"F",4)]
        [TestCase("false",typeof(bool), "F", 5)]
        [TestCase("2001-01-01",typeof(DateTime), "F", 10)]
        [TestCase("2001-01-01",typeof(DateTime), "FingersMcNultyFishBones", 23)]
        public void TestDatatypeComputer_FallbackOntoStringLength(string legitType, Type expectedLegitType, string str, int expectedLength)
        {
            DataTypeComputer t = new DataTypeComputer();
            
            //give it the legit hard typed value e.g. a date
            t.AdjustToCompensateForValue(legitType);
            Assert.AreEqual(expectedLegitType, t.CurrentEstimate);

            //then give it a string
            t.AdjustToCompensateForValue(str);
            Assert.AreEqual(typeof(string), t.CurrentEstimate);

            //the length should be the max of the length of the legit string and the string str
            Assert.AreEqual(expectedLength, t.Length);
            
        }

        [Test]
        [TestCase("-/-")]
        [TestCase("0/0")]
        [TestCase(".")]
        [TestCase("/")]
        [TestCase("-")]
        public void TestDatatypeComputer_RandomCrud(string randomCrud)
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(randomCrud);
            Assert.AreEqual(typeof(string), t.CurrentEstimate);
        }
    }
}
