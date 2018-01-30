using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    /// <summary>
    /// Calculates a DatabaseTypeRequest based on a collection of objects seen so far.  This allows you to take a DataTable column (which might be only string
    /// formatted) and identify an appropriate database type to hold the data.  For example if you see "2001-01-01" in the first row of column then the database
    /// type can be 'datetime' but if you subsequently see 'n\a' then it must become 'varchar(10)' (since 2001-01-01 is 10 characters long).
    /// 
    /// Includes support for DateTime, Timespan, String (including calculating max length), Int, Decimal (including calculating scale/precision). 
    /// 
    /// DataTypeComputer will always use the most restrictive data type possible first and then fall back on weaker types as new values are seen that do not fit
    /// the guessed Type, ultimately falling back to varchar(x).
    /// </summary>
    public class DataTypeComputer
    {
        private Type[] preferenceOrder =
        {
            typeof(bool),
            typeof (int),
            typeof (decimal),
            typeof(TimeSpan),
            typeof (DateTime), //ironically Convert.ToDateTime likes int and floats as valid dates -- nuts
            typeof (string)
        };

        public Type CurrentEstimate { get; set; }
        public int Length { get; private set; }

        public int numbersBeforeDecimalPlace { get; private set; }
        public int numbersAfterDecimalPlace { get; private set; }

        private bool acceptedTimespanAtSomePoint = false;

        /// <summary>
        /// Matches any number which looks like a proper decimal but has leading zeroes e.g. 012837 but does not match when there are
        /// decimal places or other characters e.g. it wouldn't match 0.00123.  This is used to preserve leading zeroes in integers (desired
        /// because it could be a serial number or otherwise important leading 0).  In this case the DataTypeComputer will use varchar(x) to
        /// represent the column instead of decimal(x,y)
        /// </summary>
        Regex zeroPrefixedNumber = new Regex(@"^0[\d]+$");

        /// <summary>
        /// Creates a new DataType 
        /// </summary>
        /// <param name="minimumLengthToMakeCharacterFields"></param>
        public DataTypeComputer(int minimumLengthToMakeCharacterFields)
        {
            Length = minimumLengthToMakeCharacterFields;
            numbersBeforeDecimalPlace = -1;
            numbersAfterDecimalPlace = -1;

            CurrentEstimate = preferenceOrder[0];
        }

        public DataTypeComputer():this(-1)
        {
            
        }

        /// <summary>
        /// Creates a new DataTypComputer adjusted to compensate for all values in all rows of the supplied DataColumn
        /// </summary>
        /// <param name="column"></param>
        public DataTypeComputer(DataColumn column):this(-1)
        {
            var dt = column.Table;
            foreach (DataRow row in dt.Rows)
                AdjustToCompensateForValue(row[column]);
        }

        /// <summary>
        /// Creates a hydrated DataTypComputer  for when you want to clone an existing one or otherwise make up a DataTypComputer for a known starting datatype
        /// (See TypeTranslater.GetDataTypeComputerFor)
        /// </summary>
        /// <param name="currentEstimatedType"></param>
        /// <param name="digits"></param>
        /// <param name="lengthIfString"></param>
        public DataTypeComputer(Type currentEstimatedType, Tuple<int, int> digits, int lengthIfString)
        {
            CurrentEstimate = currentEstimatedType;

            if (lengthIfString > 0)
                Length = lengthIfString;

            if (digits != null)
            {
                numbersBeforeDecimalPlace = digits.Item1;
                numbersAfterDecimalPlace = digits.Item2;
                Length = digits.Item1 + digits.Item2 + 1;
            }

            if (currentEstimatedType == typeof (TimeSpan))
                acceptedTimespanAtSomePoint = true;
        }

        private bool havereceivedBonafideType = false;

        public void AdjustToCompensateForValue(object o)
        {
            if(o == null)
                return;

            if(o == DBNull.Value)
                return;

            //if we have previously seen a hard typed value then we can't just change datatypes to something else!
            if (havereceivedBonafideType)
                if (CurrentEstimate != o.GetType())
                {
                    throw new Exception("We were adjusting to compensate for object " + o + " which is of Type " +
                                        o.GetType() + " , we were previously passed a " + CurrentEstimate +
                                        " type, is your column of mixed type? this is unacceptable");
                }

            var oAsString = o as string;
            
            //its a string, probably an untyped column :( lets work out what datatype would be suitable
            if (oAsString != null)
            {
                if(string.IsNullOrWhiteSpace(oAsString))
                    return;

                int indexOfCurrentPreference = Array.IndexOf(preferenceOrder, CurrentEstimate);

                for (int i = indexOfCurrentPreference; i <preferenceOrder.Length ; i++)
                {
                    Debug.Assert(CurrentEstimate == preferenceOrder[i]);
                    //try it as current estimate
                    bool canConvert = IsAcceptableAs(CurrentEstimate, oAsString);
                    
                    if (canConvert)
                        break;//it is acceptable
                    else
                        CurrentEstimate = preferenceOrder[i+1];//try the next type because this one isn't working -- 
                }
            }
            else
            {
                //its not a string

                //if we have yet to see a proper type
                if (!havereceivedBonafideType)
                {
                    CurrentEstimate = o.GetType();//get its type
                    havereceivedBonafideType = true;
                }

                //While it might seem obvious that o.GetType() is compatible as it's ToString() we still need to measure decimal places for types like double and Int16 etc
                IsAcceptableAs(o.GetType(), o.ToString());
            }
        }
        
        

        private bool IsAcceptableAs(Type type, string value)
        {
            //we might need to fallback on a string later on, in this case we should always record the maximum length of input seen before even if it is acceptable as int, double, dates etc
            Length = Math.Max(Length, value.Length);

            if (type == typeof (TimeSpan))
                try
                {
                    DateTime t;
                    
                    //if it parses as a date 
                    if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out t))
                    {
                        bool toReturn = (t.Year == 1 && t.Month == 1 && t.Day == 1);//without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too 
                        
                        //if we ever accept a TimeSpan then we have to fallback to string if we get a date afterwards otherwise we will be treating previous values of time only as DateTime which usually results in C#/TSQL/SQL Server using todays date for the date part which is bad
                        if (toReturn && !acceptedTimespanAtSomePoint)
                            acceptedTimespanAtSomePoint = true;
                        
                        return toReturn;
                    }
                    
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }

            if(type == typeof(DateTime))
                try
                {
                    //never accept as a date if we previously accepted as a TimeSpan
                    if (acceptedTimespanAtSomePoint)
                        return false;

                    DateTime t;
                    return DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault,out t);

                }
                catch (Exception)
                {
                    return false;
                }

            if (type == typeof (bool))
            {
                bool result;
                return bool.TryParse(value,out result);
            }

            if (type == typeof(int) || type == typeof(Int16) || type == typeof(Int64) || type == typeof(Int32))
                try
                {
                    //we must preserve leading zeroes
                    if (IsFreakilyZeroPrefixedNumber(value))
                        return false;

                    var t = Convert.ToInt32(value);
                    
                    //we could switch from int to decimal in which case we need to store number of decimal places before incase we get 1000 then 0.1 then 1 and then end we should end on decimal(5,1) not decimal(2,1)
                    numbersBeforeDecimalPlace = Math.Max(numbersBeforeDecimalPlace, t.ToString().Length);
                    
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

                if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                try
                {
                    if(IsFreakilyZeroPrefixedNumber(value))
                        return false;

                    var t = decimal.Parse(value);

                    int before;
                    int after;

                    GetDecimalPlaces(t, out before, out after);

                    //could be whole number with no decimal
                    numbersBeforeDecimalPlace = Math.Max(numbersBeforeDecimalPlace, before);
                    numbersAfterDecimalPlace = Math.Max(numbersAfterDecimalPlace, after);
                    
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            //Everything is always acceptable as a string (Length is Maxed at the top of this method).
            if (type == typeof (string))
                return true;


            if (type == typeof(byte) || type == typeof(byte[]))
            {
                //sure! whatever
                return true;
            }

            throw new NotSupportedException("Don't know how to check for acceptability of type " + type);
        }

        private bool IsFreakilyZeroPrefixedNumber(string value)
        {
             //we must preserve leading zeroes if its not actually 0 -- if they have 010101 then we have to use string but if they have just 0 we can use decimal
            if (zeroPrefixedNumber.IsMatch(value))
                    return true;

            return false;
        }

        public void GetDecimalPlaces(decimal value, out int before, out int after)
        {
            decimal destructive = Math.Abs(value);

            if (value == 0)
            {
                before = 0;
                after = 0;
                return;
            }
            
            before = 0;
            while (destructive >= 1)
            {
                destructive = destructive/10; //divide by 10
                destructive = decimal.Floor(destructive);//get rid of any overflowing decimal places 
                before++;
            }

            //always leave at least 1 before so that we can store 0s
            before = Math.Max(before, 1);

            //as if by magic... apparently
            after = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        }

        public string GetSqlDBType(DiscoveredServer server)
        {
            return GetSqlDBType(server.Helper.GetQuerySyntaxHelper().TypeTranslater);
        }

        public string GetSqlDBType(ITypeTranslater translater)
        {
            return translater.GetSQLDBTypeForCSharpType(GetTypeRequest());
        }

        public DatabaseTypeRequest GetTypeRequest()
        {
            return new DatabaseTypeRequest(
                CurrentEstimate,
                Length == -1 ? (int?) null : Length,
                new Tuple<int, int>(numbersBeforeDecimalPlace, numbersAfterDecimalPlace));
        }

        /// <summary>
        /// Returns true if the DataTypeComputer CurrentEstimate is considered to be an improvement on the DataColumn provided. Use only when you actually want to
        /// consider changing the value.  For example if you have read a CSV file into a DataTable and all current columns string/object then you can call this method
        /// to determine whether the DataTypeComputer found a more appropriate Type or not.  
        /// 
        /// Note that if you want to change the Type you need to clone the DataTable, see: https://stackoverflow.com/questions/9028029/how-to-change-datatype-of-a-datacolumn-in-a-datatable
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool ShouldDowngradeColumnTypeToMatchCurrentEstimate(DataColumn col)
        {
            if(col.DataType == typeof(object) || col.DataType == typeof(string))
            {
                int indexOfCurrentPreference = Array.IndexOf(preferenceOrder, CurrentEstimate);
                int indexOfCurrentColumn = Array.IndexOf(preferenceOrder, typeof(string));
                
                //e.g. if current preference based on data is DateTime/integer and col is a string then we SHOULD downgrade
                return indexOfCurrentPreference < indexOfCurrentColumn;
            }

            //it's not a string or an object, user probably has a type in mind for his DataColumn, let's not change that
            return false;
        }
    }
}