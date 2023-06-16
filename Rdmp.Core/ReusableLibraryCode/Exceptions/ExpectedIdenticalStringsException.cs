// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Exceptions;

/// <summary>
/// Thrown when two strings that were expected to be identical are different.  Includes ASCII art to show where they are different.
/// </summary>
public class ExpectedIdenticalStringsException : Exception
{
    public string Expected { get; set; }
    public string Actual { get; set; }

    public ExpectedIdenticalStringsException(string message, string expected, string actual)
        : base(AssembleMessage(message, expected, actual))
    {
        Expected = expected;
        Actual = actual;
    }

    private static string AssembleMessage(string message, string expected, string actual)
    {
        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual))
            return message;

        expected = expected.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        actual = actual.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

        for (var i = 0; i < expected.Length; i++)
        {
            if (!(i < actual.Length))
                return
                    $"{message}{Environment.NewLine}Strings are identical except that Actual string ends at character {i} while we still Expected {expected.Length - actual.Length} additional characters";

            //give them a preview of the location of the difference
            if (!expected[i].Equals(actual[i]))
            {
                message = $"{message}{Environment.NewLine}Strings differ at index {i}";
                message += GetPreviewsAround(i, expected, actual);
                    

                return message;
            }
        }

        return
            $"{message}{Environment.NewLine}Strings are identical except that Expected string ends at character {expected.Length} while the Actual string had {actual.Length - expected.Length} additional characters{GetPreviewsAround(expected.Length, expected, actual)}";
    }

    private static string GetPreviewsAround(int i, string expected, string actual)
    {
        var toReturn = "";

        var previewExpected = GetPreviewAround(i,expected,out var iIsAtCharacterPosition);
        var previewActual = GetPreviewAround(i, actual, out iIsAtCharacterPosition);

        var differencePointer = "";

        for (var j = 0; j < iIsAtCharacterPosition + "EXPECTED:".Length; j++)
            differencePointer += "-";

        differencePointer += "^";

        toReturn = $"{toReturn}{Environment.NewLine}EXPECTED:{previewExpected}";
        toReturn = $"{toReturn}{Environment.NewLine}ACTUAL  :{previewActual}";
        toReturn = toReturn + Environment.NewLine + differencePointer;

        return toReturn;
    }

    private static string GetPreviewAround(int i, string str, out int iIsAtCharacterPosition)
    {
        var charsBefore = 20;
        var charsAfter = 10;

        //Do not start preview before the beginning of the string e.g. if difference is at index 3 then start at 0 not -17
        var startSubstringAt = Math.Max(0, i - charsBefore);
        iIsAtCharacterPosition =
            i - startSubstringAt; //if difference is at index 350 then return 350 - (350-20) i.e. 20 but if difference is at index 3 then return 3 - 0 (see Math.Max on line above)

        var lengthWeWantToTake = i - startSubstringAt + charsAfter;//usually the full amount unless charsBefore is truncated due to the start of the string (when difference is early in the string)
        var lengthAvailable = str.Length - startSubstringAt;
        var lengthWeWillActuallyTake = Math.Min(lengthWeWantToTake, lengthAvailable);


        //if there is more available in the string put a ... so user knows it
        if (lengthAvailable > lengthWeWillActuallyTake)
            return $"{str.Substring(startSubstringAt, lengthWeWillActuallyTake)}...";

        //We ran out of characters at the end of the string so don't add ....
        return str.Substring(startSubstringAt, lengthWeWillActuallyTake);
    }
}