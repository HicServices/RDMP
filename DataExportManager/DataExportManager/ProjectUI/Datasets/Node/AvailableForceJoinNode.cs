using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Annotations;

namespace DataExportManager.ProjectUI.Datasets.Node
{
    internal class AvailableForceJoinNode : IMasqueradeAs
    {
        [CanBeNull]
        public SelectedDatasetsForcedJoin ForcedJoin { get; set; }

        public TableInfo TableInfo { get; set; }
        public JoinInfo[] JoinInfos { get; set; }
        public bool IsMandatory { get; set; }

        public AvailableForceJoinNode(TableInfo tableInfo, bool isMandatory)
        {
            TableInfo = tableInfo;
            IsMandatory = isMandatory;

            JoinInfos = ((CatalogueRepository) TableInfo.Repository).JoinInfoFinder.GetAllJoinInfosWhereTableContains(TableInfo,JoinInfoType.AnyKey);
        }

        public object MasqueradingAs()
        {
            return TableInfo;
        }

        public override string ToString()
        {
            return TableInfo.ToString();
        }

        #region Equality Members
        protected bool Equals(AvailableForceJoinNode other)
        {
            return TableInfo.Equals(other.TableInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AvailableForceJoinNode) obj);
        }

        public override int GetHashCode()
        {
            return TableInfo.GetHashCode();
        }

        #endregion
    }
}
