namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for an object that has a name that varies depending on which stage of a data load you are attempting to reference.  For example TableInfo will have a 
    /// different name depending on whether you are addressing the live table or the table as it exists in during the AdjustStaging during a data load.  Likewise an
    /// anonymised ColumnInfo will have a different name in the live stage (e.g. ANOLabNumber) vs the raw stage (e.g. LabNumber - column prior to anonymisation).
    /// 
    /// See also IHasRuntimeName
    /// </summary>
    public interface IHasStageSpecificRuntimeName
    {
        string GetRuntimeName(LoadStage stage);
    }
}