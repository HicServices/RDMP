using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public class DecimalSize
    {
        public int? NumbersBeforeDecimalPlace;
        public int? NumbersAfterDecimalPlace;

        public DecimalSize()
        {
            
        }

        public DecimalSize(int numbersBeforeDecimalPlace, int numbersAfterDecimalPlace)
        {
            NumbersBeforeDecimalPlace = Math.Max(0,numbersBeforeDecimalPlace);
            NumbersAfterDecimalPlace = Math.Max(0,numbersAfterDecimalPlace);
        }

        public bool IsEmpty
        {
            get
            {
                return Precision == 0;
                
            }
        }

        public int Precision { get { return  (NumbersBeforeDecimalPlace??0) + (NumbersAfterDecimalPlace??0); } }
        public int Scale { get { return NumbersAfterDecimalPlace??0; } }

        public void IncreaseTo(int numbersBeforeDecimalPlace)
        {
            NumbersBeforeDecimalPlace = NumbersBeforeDecimalPlace == null ? numbersBeforeDecimalPlace : Math.Max(NumbersBeforeDecimalPlace.Value, numbersBeforeDecimalPlace);
        }

        public void IncreaseTo(int numbersBeforeDecimalPlace, int numbersAfterDecimalPlace)
        {
            NumbersBeforeDecimalPlace = NumbersBeforeDecimalPlace == null ? numbersBeforeDecimalPlace : Math.Max(NumbersBeforeDecimalPlace.Value, numbersBeforeDecimalPlace);
            NumbersAfterDecimalPlace = NumbersAfterDecimalPlace == null ? numbersAfterDecimalPlace : Math.Max(NumbersAfterDecimalPlace.Value, numbersAfterDecimalPlace);
        }

        private void IncreaseTo(DecimalSize other)
        {

            if (other.NumbersBeforeDecimalPlace != null)
                NumbersBeforeDecimalPlace = NumbersBeforeDecimalPlace == null ? other.NumbersBeforeDecimalPlace : Math.Max(NumbersBeforeDecimalPlace.Value, other.NumbersBeforeDecimalPlace.Value);

            if(other.NumbersAfterDecimalPlace != null)
                NumbersAfterDecimalPlace = NumbersAfterDecimalPlace == null ? other.NumbersAfterDecimalPlace : Math.Max(NumbersAfterDecimalPlace.Value, other.NumbersAfterDecimalPlace.Value);
        }


        /// <summary>
        /// Returns the number of characters required to represent the currently computed decimal size e.g. 1.2 requries length of 3.
        /// </summary>
        /// <returns></returns>
        public int ToStringLength()
        {
            int lengthRequired = 0;

            lengthRequired += NumbersAfterDecimalPlace ?? 0;
            lengthRequired += NumbersBeforeDecimalPlace ??0;

            //if it has things after the decimal point
            if (Scale != 0)
                lengthRequired++;

            return lengthRequired;
        }

        public static DecimalSize Combine(DecimalSize first, DecimalSize second)
        {
            if (first == null)
                return second;

            if (second == null)
                return first;

            var newSize = new DecimalSize();
            newSize.IncreaseTo(first);
            newSize.IncreaseTo(second);
            
            return newSize;
        }

        #region Equality
        protected bool Equals(DecimalSize other)
        {
            return NumbersBeforeDecimalPlace == other.NumbersBeforeDecimalPlace && NumbersAfterDecimalPlace == other.NumbersAfterDecimalPlace;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DecimalSize)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NumbersBeforeDecimalPlace.GetHashCode() * 397) ^ NumbersAfterDecimalPlace.GetHashCode();
            }
        }
        #endregion
    }
}
