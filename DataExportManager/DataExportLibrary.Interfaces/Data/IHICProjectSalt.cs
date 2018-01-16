namespace DataExportLibrary.Interfaces.Data
{
    /// <summary>
    /// Provides the salt which will be passed for use by the Hashing algorithm in data extraction  (See ConfigureHashingAlgorithm). 
    /// </summary>
    public interface IHICProjectSalt
    {
        string GetSalt();
    }
}