namespace CatalogueLibrary.Data.DataLoad
{
    public interface IHasStageSpecificRuntimeName
    {
        string GetRuntimeName(LoadStage stage);
    }
}