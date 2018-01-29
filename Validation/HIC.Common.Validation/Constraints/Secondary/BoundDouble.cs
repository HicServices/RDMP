using System;
using System.ComponentModel;

namespace HIC.Common.Validation.Constraints.Secondary
{
    /// <summary>
    /// Values (if present) in a column must be within a certain range of numeric values.  This can include referencing another column.  For example you could
    /// specify that the column 'AverageResult' must have an Inclusive Upper bound of the column 'MaxResult'.
    /// </summary>
    public class BoundDouble :  Bound
    {
        [Description("Optional, Requires the value being validated to be HIGHER than this number")]
        public double? Lower { get; set; }

        [Description("Optional, Requires the value being validated to be LOWER than this number")]
        public double? Upper { get; set; }
        
        public BoundDouble()
        {
            Inclusive = true;
        }

        public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
        {
            //nulls are fine
            if(value == null)
                return null;

            //nulls are also fine if we are passed blanks
            if (value is string && string.IsNullOrWhiteSpace(value as string))
                return null;

            double v;

            try
            {
                v = Convert.ToDouble(value);
            }
            catch (FormatException)
            {
                return new ValidationFailure("Invalid format for double ",this);
            }
          

            if (Lower.HasValue || Upper.HasValue)
                if (value != null && !IsWithinRange(v))
                    return new ValidationFailure(CreateViolationReportUsingValues(v),this);

            if (value != null && !IsWithinRange(v, otherColumns, otherColumnNames))
                return new ValidationFailure(CreateViolationReportUsingFieldNames(v), this);

//            if (v < Lower || v > Upper) 
//                throw new ValidationException("Value [" + v + "] out of range. Expected a value between " + Lower + " and " + Upper + ".");

            return null;
        }

        private bool IsWithinRange(double v)
        {
            if (Inclusive)
            {
                if (Lower.HasValue && v < Lower)
                    return false;

                if (Upper.HasValue && v > Upper)
                    return false;
            }
            else
            {
                if (Lower.HasValue && v <= Lower)
                    return false;

                if (Upper.HasValue && v >= Upper)
                    return false;
            }

            return true;
        }

        private bool IsWithinRange(double d, object[] otherColumns, string[] otherColumnNames)
        {
            object low = LookupFieldNamed(LowerFieldName, otherColumns, otherColumnNames);
            object up = LookupFieldNamed(UpperFieldName, otherColumns, otherColumnNames);

            double l = Convert.ToDouble(low);
            double u = Convert.ToDouble(up);

            if (Inclusive)
            {
                if (low != null && d < l)
                    return false;

                if (up != null && d > u)
                    return false;
            }
            else
            {
                if (low != null && d <= l)
                    return false;

                if (up != null && d >= u)
                    return false;
            }

            return true;
        }

        private string CreateViolationReportUsingValues(double d)
        {
            if (Lower.HasValue && Upper.HasValue)
                return BetweenMessage(d, Lower.ToString(), Upper.ToString());

            if (Lower.HasValue)
                return GreaterThanMessage(d, Lower.ToString());

            if (Upper.HasValue)
                return LessThanMessage(d, Upper.ToString());

            throw new InvalidOperationException("Illegal state.");
        }

        private string CreateViolationReportUsingFieldNames(double d)
        {
            if (!String.IsNullOrWhiteSpace(LowerFieldName) && !String.IsNullOrWhiteSpace(UpperFieldName))
                return BetweenMessage(d, LowerFieldName, UpperFieldName);

            if (!String.IsNullOrWhiteSpace(LowerFieldName))
                return GreaterThanMessage(d, LowerFieldName);

            if (!String.IsNullOrWhiteSpace(UpperFieldName))
                return LessThanMessage(d, UpperFieldName);

            throw new InvalidOperationException("Illegal state.");
        }

        private string BetweenMessage(double d, string l, string u)
        {
            return "Value " + Wrap(d.ToString()) + " out of range. Expected a value between " + Wrap(l) + " and " + Wrap(u) + (Inclusive ? " inclusively" : " exclusively") + ".";
        }

        private string GreaterThanMessage(double d, string s)
        {
            return "Value " + Wrap(d.ToString()) + " out of range. Expected a value greater than " + Wrap(s) + ".";
        }

        private string LessThanMessage(double d, string s)
        {
            return "Value " + Wrap(d.ToString()) + " out of range. Expected a value less than " + Wrap(s) + ".";
        }

        private string Wrap(string s)
        {
            return "[" + s + "]";
        }
        
        public BoundDouble And(int upper)
        {
            Upper = upper;

            return this;
        }

        public void Incl()
        {
            Inclusive = true;
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            string result = base.GetHumanReadableDescriptionOfValidation();


            if (Lower != double.MinValue)
                if (Inclusive)
                    result += " >=" + Lower;
                else
                    result += " >" + Lower;

            if (Upper != double.MaxValue)
                if (Inclusive)
                    result += " <=" + Upper;
                else
                    result += " <" + Upper;

            return result;
        }
    }
}
