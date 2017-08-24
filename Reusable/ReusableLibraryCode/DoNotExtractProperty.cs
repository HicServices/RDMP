using System;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Used to indicate when a property should nto be extracted and passed out to researchers as metadata content (use for internal fields like ValidatorXML)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DoNotExtractProperty : Attribute
    {
    }
}