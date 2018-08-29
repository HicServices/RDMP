using System;

namespace CatalogueLibrary.Data
{

    /// <summary>
    /// Used to model Design Time initialization of IDataFlowComponents and DLE ProcessTasks (IAttacher etc).  Decorate public properties of IDataFlowComponents
    /// with this attribute to allow the user  to define values for the Pipeline when they build it.  Each Demand will be serialised as a 
    /// PipelineComponentArgument/ProcessTaskArgument.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class DemandsInitializationAttribute : System.Attribute
    {
        /// <summary>
        /// User readable description of what they are supposed to supply as values for the decorated property, allowable values etc
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Any special subcategory e.g. String might be <see cref="CatalogueLibrary.Data.DemandType.SQL"/> or it might just be a regular string value.  If in doubt
        /// use <see cref="CatalogueLibrary.Data.DemandType.Unspecified"/> (the default)
        /// </summary>
        public DemandType DemandType { get; set; }

        /// <summary>
        /// The value to use if the user hasn't picked a value yet (created an <see cref="IArgument"/>).  This will also be the default value set
        /// on any <see cref="IArgument"/> when it is created.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// True if the public property must have a value supplied by the user.  This is compatible with <see cref="DefaultValue"/>.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// If the property being decorated is System.Type e.g. <code>public Type OperationType {get;set;}</code>.  Then this specifies which Types the user can
        /// select (anything derrived from this Type).  This lets you have the user pick a strategy for your plugin as long as the strategies have blank/compatible 
        /// constructors.  You will have to decide how best to instantiate this Type yourself at runtime.
        /// </summary>
        public Type TypeOf { get; set; }

        /// <summary>
        /// Marks a public property on an RDMP plugin class as editable by the user.  The user can pick a value at design time for use with the plugin e.g. in a <see cref="Pipeline"/>
        /// then when the pipeline is run your class will be instantiated and all properties will be hydrated from the corresponding <see cref="IArguments"/>.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="demandType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="typeOf"></param>
        /// <param name="mandatory"></param>
        public DemandsInitializationAttribute(string description,DemandType demandType =  DemandType.Unspecified, object defaultValue = null, Type typeOf = null, bool mandatory = false)
        {
            Description = description;
            DemandType = demandType;
            DefaultValue = defaultValue;
            TypeOf = typeOf;
            Mandatory = mandatory;
        }
    }
}
