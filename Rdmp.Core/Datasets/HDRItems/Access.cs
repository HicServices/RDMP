using System.Collections.Generic;

namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
    /// </summary>
    public class Access
    {
        public string deliveryLeadTime { get; set; }
        public List<string> jurisdiction { get; set; }
        public string dataController { get; set; }
        public string dataProcessor { get; set; }
        public string accessRights { get; set; }
        public string accessService { get; set; }
        public string accessRequestCost { get; set; }
        public object accessServiceCategory { get; set; }
    }
}
