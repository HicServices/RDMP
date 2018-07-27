using System;
using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    /// <summary>
    /// Class responsible for deciding whether a given string representation is likely to be for a given C# Type e.g. 
    /// "2001-01-01" is likley to be a date.
    /// 
    /// <para>Each IDecideTypesForStrings should be for a single Type although different sizes is allowed e.g. Int16, Int32, Int64</para>
    /// 
    /// <para>Implementations should be as mutually exclusive with as possible.  Look also at <see cref="DatabaseTypeRequest.PreferenceOrder"/> and <see cref="DataTypeComputer"/></para>
    /// </summary>
    public interface IDecideTypesForStrings
    {
        TypeCompatibilityGroup CompatibilityGroup { get; }
        HashSet<Type> TypesSupported { get; }
        bool IsAcceptableAsType(string candidateString,DecimalSize sizeRecord);
        object Parse(string value);
    }
}
