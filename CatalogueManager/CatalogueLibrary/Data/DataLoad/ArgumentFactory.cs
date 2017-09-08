using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data.DataLoad.Exceptions;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
    public class ArgumentFactory
    {
        /// <summary>
        /// Interrogates a class via reflection and enumerates it's properties to find any that have the attribute [DemandsInitialization]
        /// Each one of these that is found is created as a ProcessTaskArgument of the appropriate Name and PropertyType under the parent ProcessTask
        /// </summary>
        /// <typeparam name="T">A class with one or more Properties marked with DemandsInitialization</typeparam>
        /// <param name="parent">The ProcessTask that owns the wrapper class, e.g. AttacherRuntimeTask would host AnySeparatorFileAttacher (which would be T) </param>
        /// <returns>Each new ProcessTaskArgument created - note that this is not the same as GetAllCustomProcessTaskArgumentsForProcess(parent) because it will not return existing ones that were already present (and therefore not created)</returns>
        public IEnumerable<IArgument> CreateArgumentsForClassIfNotExistsGeneric<T>( Func<IArgument> CreateNewArgumentUnderParent, IArgument[] existingArguments)
        {
            return CreateArgumentsForClassIfNotExistsGeneric(typeof (T),CreateNewArgumentUnderParent,existingArguments);
        }

        public IEnumerable<IArgument> CreateArgumentsForClassIfNotExistsGeneric(
            Type underlyingClassTypeForWhichArgumentsWillPopulate, Func<IArgument> CreateNewArgumentUnderParent,
            IArgument[] existingArguments)
        {
            var classType = underlyingClassTypeForWhichArgumentsWillPopulate;

            //get all the properties that must be set on AnySeparatorFileAttacher (Those marked with the attribute DemandsInitialization
            var propertiesWeHaveToSet =
                classType.GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(DemandsInitializationAttribute), true).Any())
                    .ToArray();

            if (!propertiesWeHaveToSet.Any())
                throw new NoDemandsException("Data Class " + classType.Name + " does not have any attributes marked with DemandsInitialization");


            foreach (PropertyInfo propertyInfo in propertiesWeHaveToSet)
            {
                //theres already a property with the same name
                if (existingArguments.Any(a => a.Name.Equals(propertyInfo.Name)))
                    continue;

                //create a new one
                var argument = CreateNewArgumentUnderParent();

                //set the type and name
                argument.SetType(propertyInfo.PropertyType);
                argument.Name = propertyInfo.Name;

                DemandsInitializationAttribute attribute;
                try
                {
                    attribute = (DemandsInitializationAttribute)propertyInfo.GetCustomAttributes(typeof(DemandsInitializationAttribute)).Single();
                }
                catch (Exception e)
                {
                    throw new Exception("Property " + propertyInfo.Name + " has multiple [DemandsInitialization] attributes?!", e);
                }

                argument.Description = attribute.Description;

                if (attribute.DefaultValue != null)
                    argument.SetValue(attribute.DefaultValue);

                var saveable = argument as ISaveable;

                if (saveable != null)
                    saveable.SaveToDatabase();

                yield return argument;
            }
        }
    }
}