using System;
using System.Globalization;
using HIC.Common.Validation.Constraints.Primary;

namespace HIC.Common.Validation.Constraints.Secondary.Predictor
{
    public class ChiSexPredictor : PredictionRule
    {
        public override ValidationFailure Predict(IConstraint parent,object oChi, object oGender)
        {
            if(oChi == null || oGender == null) // Null is valid
                return null;

            char sex;

            var s = oGender as string;

            if (s != null)
                sex = s.ToCharArray()[0];
            else
                if (oGender is char)
                    sex = (char)oGender;
                else
                    throw new ArgumentException("Gender must be a string or char, gender value is a " + oGender.GetType());

            var sChi = oChi as string;

            if (sChi == null)
                throw new ArgumentException("Chi was not a string (or null) object.  It was of Type " + oChi.GetType());

            if (sChi.Length == 10)
            {
                var sexDigit = (int)Char.GetNumericValue(sChi, 8);

                bool isvalid = true;

                if (sex.ToString(CultureInfo.InvariantCulture).ToUpper() != "M" && sex.ToString(CultureInfo.InvariantCulture).ToUpper() != "F") // Pass as valid if sex is not strictly specified
                    return null;

                if (sexDigit % 2 == 0 && sex == 'M')
                    isvalid = false;

                if (sexDigit % 2 == 1 && sex == 'F')
                    isvalid = false;

                if (!isvalid)
                    return new ValidationFailure("CHI sex indicator (" + sexDigit + ")  did not match associated sex field (" + sex + ")",parent);
            }

            //invalid chi, who cares
            return null;
        }
    }
}
