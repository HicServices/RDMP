using System;
using System.Collections.Generic;
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
    /// <para>Includes support for DateTime, Timespan, String (including calculating max length), Int, Decimal (including calculating scale/precision). </para>
    /// 
    /// <para>DataTypeComputer will always use the most restrictive data type possible first and then fall back on weaker types as new values are seen that do not fit
    /// the guessed Type, ultimately falling back to varchar(x).</para>
    /// </summary>
    public class DataTypeComputer
    {
        public Type CurrentEstimate { get; set; }


        private int _stringLength;

        public int Length
        {
            get { return Math.Max(_stringLength, NumbersAfterDecimalPlace + NumbersBeforeDecimalPlace + 1); }
        }

        public int NumbersBeforeDecimalPlace { get; private set; }
        public int NumbersAfterDecimalPlace { get; private set; }

        public bool IsPrimedWithBonafideType = false;

        /// <summary>
        /// Previous data types we have seen and used to adjust our CurrentEstimate.  It is important to record these, because if we see
        /// an int and change our CurrentEstimate to int then we can't change our CurrentEstimate to datetime later on because that's not
        /// compatible with int. See test TestDatatypeComputer_IntToDateTime
        /// </summary>
        HashSet<Type> _validTypesSeen = new HashSet<Type>();

        /// <summary>
        /// Types which can be converted into one another without breaking
        /// </summary>
        HashSet<Type> _compatibleTypes = new HashSet<Type>(new[]{typeof(bool),typeof(int),typeof(decimal)});

        /// <summary>
        /// Matches any number which looks like a proper decimal but has leading zeroes e.g. 012837 including.  Also matches if there is a
        /// decimal point (optionally followed by other digits).  It must match at least 2 digits at the start e.g. 01.01 would be matched
        /// but 0.01 wouldn't be matched (that's a legit float).  This is used to preserve leading zeroes in input (desired because it could
        /// be a serial number or otherwise important leading 0).  In this case the DataTypeComputer will use varchar(x) to represent the 
        /// column instead of decimal(x,y).
        /// 
        /// <para>Also allows for starting with a negative sign e.g. -01.01 would be matched as a string</para>
        /// <para>Also allows for leading / trailing whitespace</para>
        /// 
        /// </summary>
        Regex zeroPrefixedNumber = new Regex(@"^\s*-?0+[1-9]+\.?[0-9]*\s*$");

        /// <summary>
        /// Creates a new DataType 
        /// </summary>
        /// <param name="minimumLengthToMakeCharacterFields"></param>
        public DataTypeComputer(int minimumLengthToMakeCharacterFields)
        {
            _stringLength = minimumLengthToMakeCharacterFields;
            NumbersBeforeDecimalPlace = -1;
            NumbersAfterDecimalPlace = -1;

            CurrentEstimate = DatabaseTypeRequest.PreferenceOrder[0];
        }

        public DataTypeComputer():this(-1)
        {
            
        }

        /// <summary>
        /// Creates a new DataTypeComputer adjusted to compensate for all values in all rows of the supplied DataColumn
        /// </summary>
        /// <param name="column"></param>
        public DataTypeComputer(DataColumn column):this(-1)
        {
            var dt = column.Table;
            foreach (DataRow row in dt.Rows)
                AdjustToCompensateForValue(row[column]);
        }

        /// <summary>
        /// Creates a hydrated DataTypeComputer  for when you want to clone an existing one or otherwise make up a DataTypeComputer for a known starting datatype
        /// (See TypeTranslater.GetDataTypeComputerFor)
        /// </summary>
        /// <param name="currentEstimatedType"></param>
        /// <param name="digits"></param>
        /// <param name="lengthIfString"></param>
        public DataTypeComputer(Type currentEstimatedType, Tuple<int, int> digits, int lengthIfString)
        {
            CurrentEstimate = currentEstimatedType;

            if (lengthIfString > 0)
                _stringLength = lengthIfString;

            if (digits != null)
            {
                NumbersBeforeDecimalPlace = digits.Item1;
                NumbersAfterDecimalPlace = digits.Item2;
            }
        }


        public void AdjustToCompensateForValue(object o)
        {
            if(o == null)
                return;

            if(o == DBNull.Value)
                return;

            //if we have previously seen a hard typed value then we can't just change datatypes to something else!
            if (IsPrimedWithBonafideType)
                if (CurrentEstimate != o.GetType())
                {
                    throw new DataTypeComputerException("We were adjusting to compensate for object " + o + " which is of Type " +
                                        o.GetType() + " , we were previously passed a " + CurrentEstimate +
                                        " type, is your column of mixed type? this is unacceptable");
                }

            var oAsString = o as string;
            
            //its a string, probably an untyped column :( lets work out what datatype would be suitable
            if (oAsString != null)
            {
                if(string.IsNullOrWhiteSpace(oAsString))
                    return;

                //if the current estimate is unacceptable
                while (!IsAcceptableAs(CurrentEstimate, oAsString))
                {
                    //everyone loves strings, you can fit anything into them... IsAcceptableAs can change the CurrentEstimate e.g. with ChangeEstimateDirectlyToString
                    if (CurrentEstimate == typeof (string))
                        break;
                    
                    ChangeEstimateToNext();
                }

                _validTypesSeen.Add(CurrentEstimate);
            }
            else
            {
                //its not a string
                if(_validTypesSeen.Any())
                    throw new DataTypeComputerException("We were adjusting to compensate for hard Typed object '" + o + "' which is of Type " + o.GetType() + ", but previously we were passed string values, is your column of mixed type? that is unacceptable");

                //if we have yet to see a proper type
                if (!IsPrimedWithBonafideType)
                {
                    CurrentEstimate = o.GetType();//get its type
                    IsPrimedWithBonafideType = true;
                }

                //While it might seem obvious that o.GetType() is compatible as it's ToString() we still need to measure decimal places for types like double and Int16 etc
                IsAcceptableAs(o.GetType(), o.ToString());
            }
        }

        private void ChangeEstimateToNext()
        {
            int current = DatabaseTypeRequest.PreferenceOrder.IndexOf(CurrentEstimate);
            
            //if we have never seen any good data just try the next one
            if(!_validTypesSeen.Any())
                CurrentEstimate = DatabaseTypeRequest.PreferenceOrder[current + 1];
            else
            {
                //we have seen some good data before, but we have seen something that doesn't fit with the CurrentEstimate so
                //we need to degrade the Estimate to a new Type that is compatible with all the Types previously seen
                
                var nextEstiamte = DatabaseTypeRequest.PreferenceOrder[current + 1];
                
                //if the next Type is not part of the compatible Types then we must use string
                if (!_compatibleTypes.Contains(nextEstiamte))
                    CurrentEstimate = typeof (string);
                else
                    CurrentEstimate = nextEstiamte; //else we are ok with the next estimate because it is compatible
            }
        }

        private void ChangeEstimateDirectlyToString()
        {
            CurrentEstimate = typeof (string);
        }


        private bool IsAcceptableAs(Type type, string value)
        {
            //we might need to fallback on a string later on, in this case we should always record the maximum length of input seen before even if it is acceptable as int, double, dates etc
            _stringLength = Math.Max(Length, value.Length);

            if (type == typeof (TimeSpan))
                try
                {
                    DateTime t;
                    
                    //if it parses as a date 
                    if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out t))
                    {
                        return (t.Year == 1 && t.Month == 1 && t.Day == 1);//without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too 
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
                    if (_validTypesSeen.Contains(typeof(TimeSpan)))
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
                    {
                        ChangeEstimateDirectlyToString();
                        return false;
                    }

                    var t = Convert.ToInt32(value);
                    
                    //we could switch from int to decimal in which case we need to store number of decimal places before incase we get 1000 then 0.1 then 1 and then end we should end on decimal(5,1) not decimal(2,1)
                    NumbersBeforeDecimalPlace = Math.Max(NumbersBeforeDecimalPlace, t.ToString().Length);
                    
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
                    {
                        ChangeEstimateDirectlyToString();
                        return false;
                    }

                    var t = decimal.Parse(value);

                    int before;
                    int after;

                    GetDecimalPlaces(t, out before, out after);

                    //could be whole number with no decimal
                    NumbersBeforeDecimalPlace = Math.Max(NumbersBeforeDecimalPlace, before);
                    NumbersAfterDecimalPlace = Math.Max(NumbersAfterDecimalPlace, after);
                    
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
                before = 1;
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
                new Tuple<int, int>(NumbersBeforeDecimalPlace, NumbersAfterDecimalPlace));
        }

        /// <summary>
        /// Returns true if the DataTypeComputer CurrentEstimate is considered to be an improvement on the DataColumn provided. Use only when you actually want to
        /// consider changing the value.  For example if you have read a CSV file into a DataTable and all current columns string/object then you can call this method
        /// to determine whether the DataTypeComputer found a more appropriate Type or not.  
        /// 
        /// <para>Note that if you want to change the Type you need to clone the DataTable, see: https://stackoverflow.com/questions/9028029/how-to-change-datatype-of-a-datacolumn-in-a-datatable</para>
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool ShouldDowngradeColumnTypeToMatchCurrentEstimate(DataColumn col)
        {
            if(col.DataType == typeof(object) || col.DataType == typeof(string))
            {
                int indexOfCurrentPreference = DatabaseTypeRequest.PreferenceOrder.IndexOf(CurrentEstimate);
                int indexOfCurrentColumn = DatabaseTypeRequest.PreferenceOrder.IndexOf(typeof(string));
                
                //e.g. if current preference based on data is DateTime/integer and col is a string then we SHOULD downgrade
                return indexOfCurrentPreference < indexOfCurrentColumn;
            }

            //it's not a string or an object, user probably has a type in mind for his DataColumn, let's not change that
            return false;
        }
    }
}
