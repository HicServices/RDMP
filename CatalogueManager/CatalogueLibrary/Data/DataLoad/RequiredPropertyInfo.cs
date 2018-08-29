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
        /// <summary>
        /// The attribute that decorates the public property on the class who is demanding that the user provide a value (in an <see cref="IArgument"/>)
        /// </summary>
        public DemandsInitializationAttribute Demand { get; set; }

        /// <summary>
        /// The public property on the class who is demanding that the user provide a value (in an <see cref="IArgument"/>)
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Null unless the demand is for a property on a settings class of the main class e.g. MyPlugin has a property Settings marked with [DemandsNestedInitialization]
        /// and this <see cref="RequiredPropertyInfo"/> is for one of the public [DemandsInitialization] decorated properties of Settings.  If this is the case then
        ///  <see cref="ParentPropertyInfo"/> will be the root property Settings.
        /// </summary>
        public PropertyInfo ParentPropertyInfo { get; set; }

        /// <summary>
        /// Records the fact that a given public property on a class is marked with <see cref="DemandsInitializationAttribute"/> and that the user is supposed
        /// to provide a value for it in an <see cref="IArgument"/>
        /// </summary>
        /// <param name="demand"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="parentPropertyInfo"></param>
        public RequiredPropertyInfo(DemandsInitializationAttribute demand, PropertyInfo propertyInfo, PropertyInfo parentPropertyInfo = null)
        {
            Demand = demand;
            ParentPropertyInfo = parentPropertyInfo;
            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// The property name.  If the property is a nested one (i.e. DemandsNestedInitialization) then returns the full expression parent.property
        /// </summary>
        public string Name
        {
            get
            {
                return ParentPropertyInfo == null ? PropertyInfo.Name : ParentPropertyInfo.Name + "." + PropertyInfo.Name;
            }
        }
    }
}