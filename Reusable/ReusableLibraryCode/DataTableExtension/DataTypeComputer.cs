using System;
using System.Diagnostics;
using System.Globalization;

namespace ReusableLibraryCode.DataTableExtension
{
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



        private bool acceptedTimespanAtSomePoint = false;

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
                    if(DateTime.TryParse(value,CultureInfo.CurrentCulture,DateTimeStyles.NoCurrentDateDefault, out t));

                    return t > DateTime.MinValue;
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
            if (value.StartsWith("0"))
                if (!value.Equals("0") && !value.StartsWith("0."))
                    //permit use of decimal to store numbers that start with 0 if they are 0 or 0.xyz
                    return true;

            return false;
        }

        public void GetDecimalPlaces(decimal value, out int before, out int after)
        {
            decimal destructive = Math.Abs(value);

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

        public string GetSqlDBType()
        {
            return UsefulStuff.GetSQLDBTypeForCSharpType(CurrentEstimate, Length == -1?(int?) null:Length, new Tuple<int, int>(numbersBeforeDecimalPlace,numbersAfterDecimalPlace));
        }
    }
}