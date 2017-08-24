namespace ReusableLibraryCode.DataAccess
{
    public enum DataAccessContext
    {
        //do not change these! (although you can add new ones)
        DataLoad=0,
        DataExport=1,
        InternalDataProcessing=2,
        Any=3, //You can request DataLoad and receive an Any credential (because there is not a more specific one) but you cannot make a request for Any
        Logging=4
    }
}