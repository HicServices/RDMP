namespace ReusableLibraryCode
{
    /// <summary>
    /// Interface for an object which references a database location (e.g. a column or a table or a database etc).  The 'RuntimeName' is defined as an unqualified string
    /// as it could be used at runtime e.g. in an DbDataReader.  So for example a TableInfo called '[MyDb]..[MyTbl]' would have a 'RuntimeName' of 'MyTbl'.  
    /// 
    /// This also must take into account aliases so an ExtractionInformation class defined as 'UPPER([MyDb]..[MyTbl].[Name]) as CapsName' would have a 'RuntimeName' 
    /// of 'CapsName'. 
    /// </summary>
    public interface IHasRuntimeName
    {
        string GetRuntimeName();
    }
}