namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    /// <summary>
    /// Describes the translation capability of an <see cref="IDecideTypesForStrings"/> when assiging a proper C# Type to a collection of input strings.  For example "1" might be picked as a
    /// bool but then we see "2" and we can change our minds and assign the collection the Type int (<see cref="TypeCompatibilityGroup.Numerical"/>).  However if we saw a string "00:00" and
    /// decided it meant midnight (TimeSpan) we couldn't then change our minds to DateTime if we saw "2001-01-01 00:00:00" that's not going to fly.
    /// </summary>
    public enum TypeCompatibilityGroup
    {
        None,
        Exclusive,
        Numerical
    }
}
