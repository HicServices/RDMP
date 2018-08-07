namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for an object that has a name that varies depending on which stage of a data load you are attempting to reference.  For example TableInfo will have a 
    /// different name depending on whether you are addressing the live table or the table as it exists in during the AdjustStaging during a data load.  Likewise an
    /// anonymised ColumnInfo will have a different name in the live stage (e.g. ANOLabNumber) vs the raw stage (e.g. LabNumber - column prior to anonymisation).
    /// 
    /// <para>See also IHasRuntimeName</para>
    /// </summary>
    public interface IHasStageSpecificRuntimeName
    {
        /// <summary>
        /// Returns the runtime name (unqualified name e.g. "MyColumn" ) for the column/table at the given stage of a data load (RAW=>STAGING=>LIVE)
        /// <seealso cref="ReusableLibraryCode.IHasRuntimeName.GetRuntimeName"/>
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        string GetRuntimeName(LoadStage stage);
    }
}
