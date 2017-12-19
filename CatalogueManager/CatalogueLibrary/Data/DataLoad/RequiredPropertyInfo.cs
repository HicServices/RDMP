using System.Reflection;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Class for documenting properties declared as [DemandsInitialization] in a class.  Includes the DemandsInitializationAttribute (Description, Mandatory etc) and the 
    /// PropertyInfo (reflection) of the class as well as the parent propertyinfo if PropertyInfo is defined in a [DemandsNestedInitialization] sub component class of the
    /// of the class being evaluated.
    /// </summary>
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