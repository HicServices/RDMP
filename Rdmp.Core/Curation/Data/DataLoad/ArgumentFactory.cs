// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Provides methods for creating Argument values in the database for [DemandsInitialization] properties on classes
///     (See Argument).  Every public property marked with
///     [DemandsInitialization] on a plugin component will allow the user to specify a value of the appropriate type.  This
///     class will handle not only creating the IArguments
///     for a plugin component but also rationalising differences e.g. there is a new version of a class in the latest
///     plugin with different [DemandsInitialization] properties,
///     which Argument values are no longer needed and which new ones must be created to store configuration values.
///     <para>
///         Remember that a given plugin class can have multiple instances of it deployed into different pipelines with
///         different argument values.
///     </para>
/// </summary>
public class ArgumentFactory
{
    /// <inheritdoc cref="CreateArgumentsForClassIfNotExistsGeneric(Type,IArgumentHost,IArgument[])" />
    /// <typeparam name="T">A class with one or more Properties marked with DemandsInitialization</typeparam>
    /// <returns>
    ///     Each new ProcessTaskArgument created - note that it will not return existing ones that were already present
    ///     (and therefore not created)
    /// </returns>
    public static IEnumerable<IArgument> CreateArgumentsForClassIfNotExistsGeneric<T>(IArgumentHost host,
        IArgument[] existingArguments)
    {
        return CreateArgumentsForClassIfNotExistsGeneric(typeof(T), host, existingArguments);
    }

    /// <summary>
    ///     Interrogates a class via reflection and enumerates its properties to find any that have the attribute
    ///     [DemandsInitialization]
    ///     Each one of these that is found is created as a ProcessTaskArgument of the appropriate Name and PropertyType under
    ///     the parent ProcessTask
    /// </summary>
    /// <returns>
    ///     Each new ProcessTaskArgument created - note that it will not return existing ones that were already present
    ///     (and therefore not created)
    /// </returns>
    public static IEnumerable<IArgument> CreateArgumentsForClassIfNotExistsGeneric(
        Type underlyingClassTypeForWhichArgumentsWillPopulate, IArgumentHost host,
        IArgument[] existingArguments)
    {
        //get all the properties that must be set on AnySeparatorFileAttacher (Those marked with the attribute DemandsInitialization
        var propertiesWeHaveToSet = GetRequiredProperties(underlyingClassTypeForWhichArgumentsWillPopulate);

        foreach (var required in propertiesWeHaveToSet)
        {
            //there's already a property with the same name
            if (existingArguments.Any(a => a.Name.Equals(required.Name)))
                continue;

            //create a new one
            var argument = host.CreateNewArgument();

            //set the type and name
            argument.SetType(required.PropertyInfo.PropertyType);
            argument.Name = required.Name;

            var attribute = required.Demand;
            argument.Description = attribute.Description;

            if (attribute.DefaultValue != null)
                argument.SetValue(attribute.DefaultValue);

            if (argument is ISaveable saveable)
                saveable.SaveToDatabase();

            yield return argument;
        }
    }

    /// <summary>
    ///     Gets all public properties of the given <paramref name="classType" /> decorated with
    ///     <see cref="DemandsInitializationAttribute" />.
    ///     <para>
    ///         If there are any public properties encountered with <see cref="DemandsNestedInitializationAttribute" /> then
    ///         the referenced class is also investigated in the same manner.
    ///     </para>
    /// </summary>
    /// <param name="classType"></param>
    /// <returns></returns>
    public static List<RequiredPropertyInfo> GetRequiredProperties(Type classType)
    {
        var required = new List<RequiredPropertyInfo>();

        foreach (var propertyInfo in classType.GetProperties())
        {
            if (propertyInfo.GetCustomAttributes(typeof(DemandsNestedInitializationAttribute), true).Any())
            {
                var allNested = propertyInfo.PropertyType.GetProperties();
                foreach (var nestedPropInfo in allNested)
                {
                    //found a tagged attribute
                    //record the name of the property and the type it requires
                    var attribute = nestedPropInfo.GetCustomAttribute<DemandsInitializationAttribute>();

                    if (attribute != null)
                        required.Add(new RequiredPropertyInfo(attribute, nestedPropInfo, propertyInfo));
                }
            }

            var demands = propertyInfo.GetCustomAttributes(typeof(DemandsInitializationAttribute), true);

            if (demands.Length > 1)
                throw new Exception(
                    $"Property {propertyInfo} on class {classType} has multiple declarations of DemandsInitializationAttribute");

            var demand = (DemandsInitializationAttribute)demands.SingleOrDefault();

            //found a tagged attribute
            if (demand != null)
                required.Add(new RequiredPropertyInfo(demand, propertyInfo));
        }

        return required;
    }

    /// <summary>
    ///     Creates <see cref="IArgument" /> instances for all demanded properties (See <see cref="GetRequiredProperties" />)
    ///     of the given class and deletes any old arguments
    ///     which are no longer required by the class (e.g. due to an API change).
    /// </summary>
    /// <param name="host"></param>
    /// <param name="underlyingClassTypeForWhichArgumentsWillPopulate"></param>
    public static void SyncArgumentsForClass(IArgumentHost host, Type underlyingClassTypeForWhichArgumentsWillPopulate)
    {
        if (host.GetClassNameWhoArgumentsAreFor() != underlyingClassTypeForWhichArgumentsWillPopulate.FullName)
            throw new ExpectedIdenticalStringsException(
                "IArgumentHost is not currently hosting the Type requested for sync",
                host.GetClassNameWhoArgumentsAreFor(), underlyingClassTypeForWhichArgumentsWillPopulate.FullName);

        var existingArguments = host.GetAllArguments().ToList();
        var required = GetRequiredProperties(underlyingClassTypeForWhichArgumentsWillPopulate);

        //get rid of arguments that are no longer required
        foreach (var argumentsNotRequired in existingArguments.Where(e => required.All(r => r.Name != e.Name)))
            argumentsNotRequired.DeleteInDatabase();

        //create new arguments
        existingArguments.AddRange(CreateArgumentsForClassIfNotExistsGeneric(
            underlyingClassTypeForWhichArgumentsWillPopulate, host, existingArguments.ToArray()));

        //handle mismatches of Type/incompatible values / unloaded Types etc
        foreach (var r in required)
        {
            var existing = existingArguments.SingleOrDefault(e => e.Name == r.Name) ?? throw new Exception(
                $"Despite creating new Arguments for class '{underlyingClassTypeForWhichArgumentsWillPopulate}' we do not have an IArgument called '{r.Name}' in the database (host='{host}')");
            if (existing.GetSystemType() != r.PropertyInfo.PropertyType)
            {
                //user wants to fix the problem
                existing.SetType(r.PropertyInfo.PropertyType);
                existing.SaveToDatabase();
            }
        }
    }

    /// <summary>
    ///     Synchronizes all arguments (See SyncArgumentsForClass) for the supplied class (
    ///     <paramref name="underlyingClassTypeForWhichArgumentsWillPopulate" />) and returns the mapping
    ///     between <see cref="IArgument" /> (which stores the value) and public class property (
    ///     <see cref="RequiredPropertyInfo" />)
    /// </summary>
    /// <param name="host"></param>
    /// <param name="underlyingClassTypeForWhichArgumentsWillPopulate"></param>
    /// <returns></returns>
    public static Dictionary<IArgument, RequiredPropertyInfo> GetDemandDictionary(IArgumentHost host,
        Type underlyingClassTypeForWhichArgumentsWillPopulate)
    {
        var toReturn = new Dictionary<IArgument, RequiredPropertyInfo>();

        SyncArgumentsForClass(host, underlyingClassTypeForWhichArgumentsWillPopulate);

        var required = GetRequiredProperties(underlyingClassTypeForWhichArgumentsWillPopulate);

        foreach (var key in host.GetAllArguments())
            toReturn.Add(key, required.Single(e => e.Name == key.Name));

        return toReturn;
    }
}