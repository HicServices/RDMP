namespace Rdmp.Core.QueryBuilding
{
    public class QueryBuilderCustomArgs
    {
        public string OverrideSelectList { get; set; }
        public string OverrideLimitationSQL { get; set; }
        public int TopX { get; set; } = -1;

        public QueryBuilderCustomArgs(string overrideSelectList,string overrideLimitationSQL,int topX)
        {
            OverrideSelectList = overrideSelectList;
            OverrideLimitationSQL = overrideLimitationSQL;
            TopX = topX;
        }

        public QueryBuilderCustomArgs()
        {
            
        }

        /// <summary>
        /// Populates <paramref name="other"/> with the values stored in this
        /// </summary>
        /// <param name="other"></param>
        public void Populate(QueryBuilderCustomArgs other)
        {
            other.OverrideLimitationSQL = OverrideLimitationSQL;
            other.OverrideSelectList = OverrideSelectList;
            other.TopX = TopX;
        }
    }
}