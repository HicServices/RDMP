using System.Reflection;

namespace CatalogueLibrary.Data.DataLoad
{
    public class RequiredPropertyInfo
    {
        public DemandsInitializationAttribute Demand { get; set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public PropertyInfo ParentPropertyInfo { get; set; }

        public RequiredPropertyInfo(DemandsInitializationAttribute demand, PropertyInfo propertyInfo, PropertyInfo parentPropertyInfo = null)
        {
            Demand = demand;
            ParentPropertyInfo = parentPropertyInfo;
            PropertyInfo = propertyInfo;
        }

        public string Name
        {
            get
            {
                return ParentPropertyInfo == null ? PropertyInfo.Name : ParentPropertyInfo.Name + "." + PropertyInfo.Name;
            }
        }
    }
}