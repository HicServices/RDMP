using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    internal abstract class DecideTypesForStrings :IDecideTypesForStrings
    {
        public TypeCompatibilityGroup CompatibilityGroup { get; private set; }
        public HashSet<Type> TypesSupported { get; private set; }

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

        protected DecideTypesForStrings(TypeCompatibilityGroup compatibilityGroup,params Type[] typesSupportedSupported)
        {
            CompatibilityGroup = compatibilityGroup;
            
            if(typesSupportedSupported.Length == 0)
                throw new ArgumentException("There must be at least one supported type");
            
            TypesSupported = new HashSet<Type>(typesSupportedSupported);
        }

        public TypeDeciderResult IsAcceptableAsType(string candidateString)
        {
            //we must preserve leading zeroes if its not actually 0 -- if they have 010101 then we have to use string but if they have just 0 we can use decimal
            if (zeroPrefixedNumber.IsMatch(candidateString))
                return new TypeDeciderResult(false);

            return IsAcceptableAsTypeImpl(candidateString);
        }

        protected abstract TypeDeciderResult IsAcceptableAsTypeImpl(string candidateString);
    }
}