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
        public string Description { get; private set; }
        public DemandType DemandType { get; set; }
        public object DefaultValue { get; set; }
        public bool Mandatory { get; set; }

        public Type TypeOf { get; set; }

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
