using System;
using System.Collections.Generic;
using System.Data;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders;

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

        /// <summary>
        /// The minimum amount of characters required to represent date values stored in the database when issuing ALTER statement to convert
        /// the column to allow strings.
        /// </summary>
        public const int MinimumLengthRequiredForDateStringRepresentation = 27;

        public Type CurrentEstimate { get; set; }
        
        private readonly TypeDeciderFactory _typeDeciders = new TypeDeciderFactory();
        
        private int _stringLength;

        public int Length
        {
            get { return Math.Max(_stringLength, DecimalSize.ToStringLength()); }
        }

        public DecimalSize DecimalSize = new DecimalSize();

        public bool IsPrimedWithBonafideType = false;

        /// <summary>
        /// Previous data types we have seen and used to adjust our CurrentEstimate.  It is important to record these, because if we see
        /// an int and change our CurrentEstimate to int then we can't change our CurrentEstimate to datetime later on because that's not
        /// compatible with int. See test TestDatatypeComputer_IntToDateTime
        /// </summary>
        TypeCompatibilityGroup _validTypesSeen = TypeCompatibilityGroup.None;

        /// <summary>
        /// Creates a new DataType 
        /// </summary>
        /// <param name="minimumLengthToMakeCharacterFields"></param>
        public DataTypeComputer(int minimumLengthToMakeCharacterFields)
        {
            _stringLength = minimumLengthToMakeCharacterFields;

            CurrentEstimate = DatabaseTypeRequest.PreferenceOrder[0];
            
        }

        public DataTypeComputer():this(-1)
        {
            
        }

        public DataTypeComputer(DatabaseTypeRequest request): this(request.MaxWidthForStrings.HasValue? request.MaxWidthForStrings.Value:-1)
        {
            CurrentEstimate = request.CSharpType;
            if (request.DecimalPlacesBeforeAndAfter != null)
                DecimalSize = request.DecimalPlacesBeforeAndAfter;

            ThrowIfNotSupported(CurrentEstimate);
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
        /// <param name="decimalSize"></param>
        /// <param name="lengthIfString"></param>
        public DataTypeComputer(Type currentEstimatedType, DecimalSize decimalSize, int lengthIfString):this(-1)
        {
            CurrentEstimate = currentEstimatedType;
            
            ThrowIfNotSupported(CurrentEstimate);

            if (lengthIfString > 0)
                _stringLength = lengthIfString;

            DecimalSize = decimalSize ?? new DecimalSize();
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

            var oToString = o.ToString();
            var oAsString = o as string;

            //we might need to fallback on a string later on, in this case we should always record the maximum length of input seen before even if it is acceptable as int, double, dates etc
            _stringLength = Math.Max(Length, oToString.Length);

            //if it's a string
            if (oAsString != null)
            {
                //ignore empty ones
                if (string.IsNullOrWhiteSpace(oAsString))
                    return;
                
                //if we have already fallen back to string then just stick with it (theres no going back up the ladder)
                if(CurrentEstimate == typeof(string))
                    return;

                var result = _typeDeciders.Dictionary[CurrentEstimate].IsAcceptableAsType(oAsString, DecimalSize);
                
                //if the current estimate compatible
                if (result)
                {
                    _validTypesSeen = _typeDeciders.Dictionary[CurrentEstimate].CompatibilityGroup;

                    if (CurrentEstimate == typeof (DateTime))
                        _stringLength = Math.Max(_stringLength, MinimumLengthRequiredForDateStringRepresentation);


                    return;
                }

                //if it isn't compatible, try the next Type
                ChangeEstimateToNext();

                //recurse because why not
                AdjustToCompensateForValue(oAsString);
            }
            else
            {
                //if we ever made a descision about a string inputs then we won't accept hard typed objects now
                if(_validTypesSeen != TypeCompatibilityGroup.None || CurrentEstimate == typeof(string))
                    throw new DataTypeComputerException("We were adjusting to compensate for hard Typed object '" + o + "' which is of Type " + o.GetType() + ", but previously we were passed string values, is your column of mixed type? that is unacceptable");

                //if we have yet to see a proper type
                if (!IsPrimedWithBonafideType)
                {
                    CurrentEstimate = o.GetType();//get its type
                    IsPrimedWithBonafideType = true;
                }

                //if we have a decider for this lets get it to tell us the decimal places (if any)
                if (_typeDeciders.Dictionary.ContainsKey(o.GetType()))
                    _typeDeciders.Dictionary[o.GetType()].IsAcceptableAsType(oToString, DecimalSize);
            }
        }

        private void ChangeEstimateToNext()
        {
            int current = DatabaseTypeRequest.PreferenceOrder.IndexOf(CurrentEstimate);
            
            //if we have never seen any good data just try the next one
            if(_validTypesSeen == TypeCompatibilityGroup.None )
                CurrentEstimate = DatabaseTypeRequest.PreferenceOrder[current + 1];
            else
            {
                //we have seen some good data before, but we have seen something that doesn't fit with the CurrentEstimate so
                //we need to degrade the Estimate to a new Type that is compatible with all the Types previously seen
                
                var nextEstiamte = DatabaseTypeRequest.PreferenceOrder[current + 1];

                //if the next estimate is a string or we have previously accepted an exclusive decider (e.g. DateTime)
                if (nextEstiamte == typeof (string) || _validTypesSeen == TypeCompatibilityGroup.Exclusive)
                    CurrentEstimate = typeof (string); //then just go with string
                else
                {
                    //if the next decider is in the same group as the previously used ones
                    if (_typeDeciders.Dictionary[nextEstiamte].CompatibilityGroup == _validTypesSeen)
                        CurrentEstimate = nextEstiamte;
                    else
                        CurrentEstimate = typeof (string); //the next Type decider is in an incompatible category so just go directly to string
                }
            }
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
                DecimalSize);
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

        private void ThrowIfNotSupported(Type currentEstimate)
        {
            if (currentEstimate == typeof(string))
                return;

            if (!_typeDeciders.IsSupported(CurrentEstimate))
                throw new NotSupportedException("We do not have a type decider for type:" + CurrentEstimate);
        }

    }
}
