using System;
using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    interface IDecideTypesForStrings
    {
        TypeCompatibilityGroup CompatibilityGroup { get; }
        HashSet<Type> TypesSupported { get; }
        TypeDeciderResult IsAcceptableAsType(string candidateString);
    }
}
