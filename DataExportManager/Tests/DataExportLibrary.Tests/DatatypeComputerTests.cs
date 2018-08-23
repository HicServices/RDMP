using System;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Type = System.Type;

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
            
            t.AdjustToCompensateForValue("0");
            Assert.AreEqual(typeof(int), t.CurrentEstimate);
            Assert.AreEqual(1, t.Length);

            t.AdjustToCompensateForValue("-0");
            Assert.AreEqual(typeof(int), t.CurrentEstimate);
            Assert.AreEqual(2, t.Length);
            
            
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
            var sqlType = t.GetSqlDBType(_translater);
            Assert.AreEqual("decimal(4,1)",sqlType) ;

            var orig = t.GetTypeRequest();
            var reverseEngineered = _translater.GetDataTypeRequestForSQLDBType(sqlType);
            Assert.AreEqual(orig,reverseEngineered ,"The computed DataTypeRequest was not the same after going via sql datatype and reverse engineering");
        }
        [Test]
        public void TestDatatypeComputer_IntAnddecimal_MustUsedecimalThenString()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("15");
            t.AdjustToCompensateForValue("29.9");
            t.AdjustToCompensateForValue("200");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);
            
            Assert.AreEqual("decimal(4,1)", t.GetSqlDBType(_translater));
            t.AdjustToCompensateForValue("D");
            Assert.AreEqual("varchar(5)", t.GetSqlDBType(_translater));
        }

        public void TestDatatypeComputer_DateTimeFromInt()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("01/01/2001");
            Assert.AreEqual(typeof(DateTime), t.CurrentEstimate);

            t.AdjustToCompensateForValue("2013");
            Assert.AreEqual(typeof(string), t.CurrentEstimate);
        }

        //Tests system being happy to sign off in the orders bool=>int=>decimal but nothing else
        [TestCase("true", typeof(bool), "11", typeof(int))]
        [TestCase("1", typeof(int), "1.1",typeof(decimal))]
        [TestCase("true", typeof(bool), "1.1", typeof(decimal))]
        public void TestDataTypeComputer_FallbackCompatible(string input1, Type expectedTypeAfterFirstInput, string input2, Type expectedTypeAfterSecondInput)
        {
            var t = new DataTypeComputer();
            t.AdjustToCompensateForValue(input1);
            
            Assert.AreEqual(expectedTypeAfterFirstInput,t.CurrentEstimate);
            
            t.AdjustToCompensateForValue(input2);
            Assert.AreEqual(expectedTypeAfterSecondInput, t.CurrentEstimate);
        }

        //Tests system being angry at having signed off on a bool=>int=>decimal then seeing a valid non string type (e.g. DateTime)
        //under these circumstances it should go directly to System.String
        [TestCase("1",typeof(int),"2001-01-01")]
        [TestCase("true", typeof(bool), "2001-01-01")]
        [TestCase("1.1", typeof(decimal), "2001-01-01")]
        [TestCase("1.1", typeof(decimal), "10:00am")]
        [TestCase("2001-1-1", typeof(DateTime), "10:00am")]
        public void TestDataTypeComputer_FallbackIncompatible(string input1, Type expectedTypeAfterFirstInput, string input2)
        {
            var t = new DataTypeComputer();
            t.AdjustToCompensateForValue(input1);

            Assert.AreEqual(expectedTypeAfterFirstInput, t.CurrentEstimate);

            t.AdjustToCompensateForValue(input2);
            Assert.AreEqual(typeof(string), t.CurrentEstimate);

            //now check it in reverse just to be sure
            t = new DataTypeComputer();
            t.AdjustToCompensateForValue(input2);
            t.AdjustToCompensateForValue(input1);
            Assert.AreEqual(typeof(string),t.CurrentEstimate);
        }

        [Test]
        public void TestDatatypeComputer_IntToDateTime()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("2013");
            t.AdjustToCompensateForValue("01/01/2001");
            Assert.AreEqual(typeof(string), t.CurrentEstimate);
        }

        [TestCase("fish",32)]
        [TestCase(32, "fish")]
        [TestCase("2001-01-01",2001)]
        [TestCase(2001, "2001-01-01")]
        [TestCase("2001", 2001)]
        [TestCase(2001, "2001")]
        public void TestDatatypeComputer_MixingTypes_ThrowsException(object o1, object o2)
        {
            //if we pass an hard type...
            //...then we don't accept strings anymore

            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(o1); 

            var ex = Assert.Throws<DataTypeComputerException>(() => t.AdjustToCompensateForValue(o2)); 
            Assert.IsTrue(ex.Message.Contains("mixed type"));
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

        [TestCase("1. 01 ", typeof(DateTime))]
        [TestCase("1. 1 ", typeof(DateTime))]
        public void TestDatatypeComputer_DateTime_DodgyFormats(string input, Type expectedOutput)
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(input);
            Assert.AreEqual(expectedOutput, t.CurrentEstimate);
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

        [TestCase("01",2)]
        [TestCase("01.1", 4)]
        [TestCase("01.10", 5)]
        [TestCase("-01", 3)]
        [TestCase("-01.01", 6)]
        [TestCase(" -01.01", 7)]
        [TestCase("\t-01.01", 7)]
        [TestCase("\r\n-01.01", 8)]
        [TestCase("- 01.01", 7)]
        [TestCase(" -01.01 ", 8)]
        [TestCase("-01.01 ", 7)]
        [TestCase("--01", 4)]
        public void TestDatatypeComputer_PreeceedingZeroes(string input, int expectedLength)
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(input);
            Assert.AreEqual(typeof(string), t.CurrentEstimate);
            Assert.AreEqual(expectedLength, t.Length);
        }

        [Test]
        public void TestDatatypeComputer_PreeceedingZeroesAfterFloat()
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

            Assert.AreEqual(2, t.DecimalSize.NumbersAfterDecimalPlace);
            Assert.AreEqual(3, t.DecimalSize.NumbersBeforeDecimalPlace);
        }

        [TestCase(" 1.01", typeof(decimal))]
        [TestCase(" 1.01 ", typeof(decimal))]
        [TestCase(" 1", typeof(int))]
        [TestCase(" true ",typeof(bool))]
        public void TestDatatypeComputer_Whitespace(string input, Type expectedType)
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(input);

            Assert.AreEqual(expectedType, t.CurrentEstimate);
            Assert.AreEqual(input.Length,t.Length);
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

            Assert.AreEqual(null, t.DecimalSize.NumbersAfterDecimalPlace);
            Assert.AreEqual(null, t.DecimalSize.NumbersBeforeDecimalPlace);
        }

        [Test]
        public void TestDatatypeComputer_MixedIntTypes()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue((Int16)5);
            var ex = Assert.Throws<DataTypeComputerException>(()=>t.AdjustToCompensateForValue((Int32)1000));

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

            Assert.AreEqual(3, t.DecimalSize.NumbersBeforeDecimalPlace);
            Assert.AreEqual(null, t.DecimalSize.NumbersAfterDecimalPlace);
            

        }
        [Test]
        public void TestDatatypeComputer_Byte()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(new byte[5]);

            Assert.AreEqual(typeof(byte[]), t.CurrentEstimate);

            Assert.AreEqual(null, t.DecimalSize.NumbersAfterDecimalPlace);
            Assert.AreEqual(null, t.DecimalSize.NumbersBeforeDecimalPlace);
            Assert.IsTrue(t.DecimalSize.IsEmpty);
        }


        [Test]
        public void TestDatatypeComputer_NumberOfDecimalPlaces()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("111111111.11111111111115");

            Assert.AreEqual(typeof(decimal), t.CurrentEstimate);
            Assert.AreEqual(9, t.DecimalSize.NumbersBeforeDecimalPlace);
            Assert.AreEqual(14, t.DecimalSize.NumbersAfterDecimalPlace);
        }
        

        [Test]
        public void TestDatatypeComputer_TrailingZeroesFallbackToString()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("-111.000");
            
            Assert.AreEqual(typeof(decimal), t.CurrentEstimate);
            Assert.AreEqual(3, t.DecimalSize.NumbersBeforeDecimalPlace);

            //even though they are trailing zeroes we still need this much space... there must be a reason why they are there right? (also makes it easier to go to string later if needed eh!)
            Assert.AreEqual(3, t.DecimalSize.NumbersAfterDecimalPlace); 
            
            t.AdjustToCompensateForValue("P");

            Assert.AreEqual(typeof(string), t.CurrentEstimate);
            Assert.AreEqual(8, t.Length);
        }

        [Test]
        public void TestDataTypeComputer_IntFloatString()
        {
            MicrosoftSQLTypeTranslater tt = new MicrosoftSQLTypeTranslater();

            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue("-1000");

            Assert.AreEqual("int",t.GetSqlDBType(tt));

            t.AdjustToCompensateForValue("1.1");
            Assert.AreEqual("decimal(6,1)", t.GetSqlDBType(tt));

            t.AdjustToCompensateForValue("A");
            Assert.AreEqual("varchar(7)", t.GetSqlDBType(tt));
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

        [Test]
        public void TestDatatypeComputer_HardTypeFloats()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(1.1f);
            t.AdjustToCompensateForValue(100.01f);
            t.AdjustToCompensateForValue(10000f);

            Assert.AreEqual(typeof(float), t.CurrentEstimate);
            Assert.AreEqual(2,t.DecimalSize.NumbersAfterDecimalPlace);
            Assert.AreEqual(5, t.DecimalSize.NumbersBeforeDecimalPlace);
        }

        [Test]
        public void TestDatatypeComputer_HardTypeInts()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(1);
            t.AdjustToCompensateForValue(100);
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(10000);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(int), t.CurrentEstimate);
            Assert.AreEqual(null, t.DecimalSize.NumbersAfterDecimalPlace);
            Assert.AreEqual(5, t.DecimalSize.NumbersBeforeDecimalPlace);
        }


        [Test]
        public void TestDatatypeComputer_HardTypeDoubles()
        {
            DataTypeComputer t = new DataTypeComputer();
            t.AdjustToCompensateForValue(1.1);
            t.AdjustToCompensateForValue(100.203);
            t.AdjustToCompensateForValue(100.20000);
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(10000d);//<- d is required because Types must be homogenous
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(double), t.CurrentEstimate);
            Assert.AreEqual(3, t.DecimalSize.NumbersAfterDecimalPlace);
            Assert.AreEqual(5, t.DecimalSize.NumbersBeforeDecimalPlace);
        }


        [TestCase("0.01",typeof(decimal),"A",4)]
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


        [TestCase("bigint")]
        [TestCase("binary")]
        [TestCase("bit")]
        [TestCase("char")]
        [TestCase("date")]
        [TestCase("datetime")]
        [TestCase("datetime2")]
        [TestCase("datetimeoffset")]
        [TestCase("decimal")]
        [TestCase("varbinary(max)")]
        [TestCase("float")]
        [TestCase("image")]
        [TestCase("int")]
        [TestCase("money")]
        [TestCase("nchar")]
        [TestCase("ntext")]
        [TestCase("numeric")]
        [TestCase("nvarchar")]
        [TestCase("real")]
        [TestCase("rowversion")]
        [TestCase("smalldatetime")]
        [TestCase("smallint")]
        [TestCase("smallmoney")]
        [TestCase("text")]
        [TestCase("time")]
        [TestCase("timestamp")]
        [TestCase("tinyint")]
        [TestCase("uniqueidentifier")]
        [TestCase("varbinary")]
        [TestCase("varchar")]
        [TestCase("xml")]
        public void TestIsKnownType(string sqlType)
        {
            var tt = new MicrosoftSQLTypeTranslater();
            var t = tt.GetCSharpTypeForSQLDBType(sqlType);

            Assert.IsNotNull(t);
        }

        [TestCase("sql_variant")]
        public void TestNotSupportedTypes(string sqlType)
        {
            var tt = new MicrosoftSQLTypeTranslater();
            Assert.Throws<NotSupportedException>(() => tt.GetCSharpTypeForSQLDBType(sqlType));
        }

    }
}
