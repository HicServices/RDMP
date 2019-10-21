using System;
using System.Reflection;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Describes a single <see cref="ParameterInfo"/> or <see cref="PropertyInfo"/> required by a <see cref="CommandInvoker"/>
    /// </summary>
    public class RequiredArgument
    {
        public string Name { get; }
        public Type Type { get; }
        public object ReflectionObject { get; }
        public bool HasDefaultValue { get;}
        public object DefaultValue { get; }

        public RequiredArgument(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            Type = propertyInfo.PropertyType;
            ReflectionObject = propertyInfo;
            HasDefaultValue = false;
            DefaultValue = null;
        }

        public RequiredArgument(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            Type = parameterInfo.ParameterType;
            ReflectionObject = parameterInfo;
            HasDefaultValue = parameterInfo.HasDefaultValue;
            DefaultValue = parameterInfo.DefaultValue;
        }
    }
}