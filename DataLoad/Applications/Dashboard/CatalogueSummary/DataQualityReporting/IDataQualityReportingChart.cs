using DataQualityEngine.Data;

namespace Dashboard.CatalogueSummary.DataQualityReporting
{
    public interface IDataQualityReportingChart
    {
        void ClearGraph();
        void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue);
    }
}