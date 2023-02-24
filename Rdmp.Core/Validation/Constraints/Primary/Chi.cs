// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text.RegularExpressions;

namespace Rdmp.Core.Validation.Constraints.Primary;

/// <summary>
/// Field must contain a chi number, this is a 10 digit number in which the first 6 digits are the patients date of birth and the last 2 digits are
/// a gender digit and a checksum.  Validation will fail if the checksum is invalid or the value does not match the pattern.
/// </summary>
public class Chi : PrimaryConstraint
{
    public override ValidationFailure Validate(object value)
    {
        if (value == null)
            return null;


        if(value is not string valueAsString)
            return new ValidationFailure(
                $"Incompatible type, CHIs must be strings, value passed was of type {value.GetType().Name}",this);

        if (!IsValidChi(valueAsString, out var reason))
            return new ValidationFailure(reason,this);
           
        return null;
    }

        
    public override void RenameColumn(string originalName, string newName)
    {
            
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return
            "Checks that the input value is 10 characters long and the first 6 characters are a valid date and that the final digit checksum matches";
    }

    public static bool IsValidChi(string columnValueAsString, out string reason)
    {
        if (columnValueAsString.Length != 10)
        {
            reason = "CHI was not 10 characters long";
            return false;
        }

        var dd = columnValueAsString[..2];
        var mm = columnValueAsString.Substring(2, 2);
        var yy = columnValueAsString.Substring(4, 2);

        if (DateTime.TryParse($"{dd}/{mm}/{yy}", out _) == false)
        {
            reason = "First 6 numbers of CHI did not constitute a valid date";
            return false;
        }


        if (columnValueAsString[^1..] != GetCHICheckDigit(columnValueAsString))
        {
            reason = "CHI check digit did not match";
            return false;
        }

        reason = null;
        return true;

    }

    /// <summary>
    /// Does the CHI check digit calculation
    /// </summary>
    /// <param name="sCHI"></param>
    /// <returns></returns>
    private static string GetCHICheckDigit(string sCHI)
    {
        int sum = 0, c = 0, lsCHI = 0;

        //sCHI = "120356785";
        lsCHI = sCHI.Length; // Must be 10!!

        sum = 0;
        c = (int)'0';
        for (var i = 0; i < lsCHI - 1; i++)
            sum += ((int)sCHI.Substring(i, 1)[0] - c) * (lsCHI - i);
        sum %= 11;

        c = 11 - sum;
        if (c == 11) c = 0;

        return ((char)(c + (int)'0')).ToString();

    }

    /// <summary>
    /// Return the sex indicated by the supplied CHI
    /// </summary>
    /// <param name="chi"></param>
    /// <returns>1 for male and 0 for female</returns>
    public int GetSex(string chi)
    {
        string errorReport;

        if (!IsValidChiNumber(chi, out errorReport))
            throw new ArgumentException("Invalid CHI");

        var sexChar = chi[8];

        return (int)(sexChar % 2);
    }

    /// <summary>
    /// Check the validity of the supplied CHI
    /// </summary>
    /// <param name="strChi"></param>
    /// <param name="errorReport"></param>
    /// <returns>true if the CHI is valid, false otherwise</returns>
    public static bool IsValidChiNumber(string strChi, out string errorReport)
    {
        errorReport = "Not yet implemented";

        if (!isWellFormedChi(strChi))
            return false;

        // Value of 10 indicates a checksum error
        var checkDigit = ComputeChecksum(strChi);

        return checkDigit != 10 && (int)char.GetNumericValue(strChi[9]) == checkDigit;
    }

    private static bool isWellFormedChi(string strChi)
    {
        if (strChi == null || strChi.Length != 10)
            return false;

        var r = new Regex("^[0-9]{10}$");
        if (!r.IsMatch(strChi))
            return false;

        return true;
    }

    private static int ComputeChecksum(string chi)
    {
        var sum = SumDigits(chi);
        var checkDigit = 0;

        var n = 11 - sum % 11;
        if (n < 10)
            checkDigit = n;

        return checkDigit;
    }

    private static int SumDigits(string chi)
    {
        var sum = 0;
        var factor = 10;
        for (var i = 0; i < 9; i++)
        {
            sum += (chi[i] - 48) * factor--;
        }

        return sum;
    }

}