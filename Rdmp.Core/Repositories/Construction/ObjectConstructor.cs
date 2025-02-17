// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Repositories.Construction;

/// <summary>
/// Simplifies identifying and invoking ConstructorInfos on Types (reflection).  This includes identifying a suitable Constructor on a class Type based on the
/// provided parameters and invoking it.  Also implicitly supports hypotheticals e.g. 'here's a TableInfo, construct class X with the TableInfo parameter or if
/// it has a blank constructor that's fine too or if it takes ITableInfo that's fine too... just use whatever works'.  If there are multiple matching constructors
/// it will attempt to find the 'best' (See InvokeBestConstructor for implementation).
/// 
/// <para>If there are no compatible constructors you will get an ObjectLacksCompatibleConstructorException.</para>
/// </summary>
public class ObjectConstructor
{
    private const BindingFlags TargetBindingFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Constructs a new instance of Type t using the blank constructor
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static object Construct(Type t) => GetUsingBlankConstructor(t);

    #region permissible constructor signatures for use with this class

    /// <summary>
    /// Constructs a new instance of Type t using the default constructor or one that takes an IRDMPPlatformRepositoryServiceLocator (or any derived class)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="serviceLocator"></param>
    /// <param name="allowBlank"></param>
    /// <returns></returns>
    public static object
        Construct(Type t, IRDMPPlatformRepositoryServiceLocator serviceLocator, bool allowBlank = true) =>
        Construct<IRDMPPlatformRepositoryServiceLocator>(t, serviceLocator, allowBlank);

    /// <summary>
    /// Constructs a new instance of Type t using the default constructor or one that takes an ICatalogueRepository (or any derived class)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="catalogueRepository"></param>
    /// <param name="allowBlank"></param>
    /// <returns></returns>
    public static object Construct(Type t, ICatalogueRepository catalogueRepository, bool allowBlank = true) =>
        Construct<ICatalogueRepository>(t, catalogueRepository, allowBlank);

    /// <summary>
    /// Constructs a new instance of Type objectType by invoking the constructor MyClass(IRepository x, DbDataReader r) (See <see cref="DatabaseEntity"/>).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="objectType"></param>
    /// <param name="repositoryOfTypeT"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static IMapsDirectlyToDatabaseTable ConstructIMapsDirectlyToDatabaseObject<T>(Type objectType,
        T repositoryOfTypeT, DbDataReader reader) where T : IRepository
    {
        // Preferred constructor
        var constructors = GetConstructors<T, DbDataReader>(objectType);

        if (!constructors.Any())
            // Fallback constructor
            throw new ObjectLacksCompatibleConstructorException(
                $"{objectType.Name} requires a constructor ({typeof(T).Name} repo, DbDataReader reader) to be used with ConstructIMapsDirectlyToDatabaseObject");

        return (IMapsDirectlyToDatabaseTable)InvokeBestConstructor(constructors, repositoryOfTypeT, reader);
    }

    #endregion

    /// <summary>
    /// Constructs an instance of object of Type 'typeToConstruct' which should have a compatible constructor taking an object or interface compatible with T
    /// or a blank constructor (optionally)
    /// </summary>
    /// <typeparam name="T">The parameter type expected to be in the constructor</typeparam>
    /// <param name="typeToConstruct">The type to construct an instance of</param>
    /// <param name="constructorParameter1">a value to feed into the compatible constructor found for Type typeToConstruct in order to produce an instance</param>
    /// <param name="allowBlank">true to allow calling the blank constructor if no matching constructor is found that takes a T</param>
    /// <returns></returns>
    public static object Construct<T>(Type typeToConstruct, T constructorParameter1, bool allowBlank = true)
    {
        var repositoryLocatorConstructorInfos = GetConstructors<T>(typeToConstruct);

        if (repositoryLocatorConstructorInfos.Any())
            return InvokeBestConstructor(repositoryLocatorConstructorInfos, constructorParameter1);
        if (allowBlank)
            try
            {
                return GetUsingBlankConstructor(typeToConstruct);
            }
            catch (ObjectLacksCompatibleConstructorException)
            {
                throw new ObjectLacksCompatibleConstructorException(
                    $"Type '{typeToConstruct}' does not have a constructor taking an {typeof(T)} - it doesn't even have a blank constructor!");
            }

        throw new ObjectLacksCompatibleConstructorException(
            $"Type '{typeToConstruct}' does not have a constructor taking an {typeof(T)}");
    }

    private static List<ConstructorInfo> GetConstructors<T>(Type type)
    {
        var toReturn = new List<ConstructorInfo>();

        foreach (var constructor in type.GetConstructors(TargetBindingFlags))
        {
            var p = constructor.GetParameters();

            switch (p.Length)
            {
                //is it an exact match i.e. ctor(T bob)
                case 1 when p[0].ParameterType == typeof(T): // Exact match found
                    return new List<ConstructorInfo>(new[] { constructor });
                case 1:
                    {
                        if (p[0].ParameterType
                            .IsAssignableFrom(
                                typeof(T))) //is it a derived class match i.e. ctor(F bob) where F is a derived class of T
                            toReturn.Add(constructor);
                        break;
                    }
            }
        }

        return toReturn;
    }


    /// <summary>
    /// Returns all constructors defined for class 'type' that are compatible with the parameters T and T2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    private static List<ConstructorInfo> GetConstructors<T, T2>(Type type)
    {
        var toReturn = new List<ConstructorInfo>();

        foreach (var constructor in type.GetConstructors(TargetBindingFlags))
        {
            var p = constructor.GetParameters();

            switch (p.Length)
            {
                case 2 when p[0].ParameterType == typeof(T) && p[1].ParameterType == typeof(T2): // Exact match found
                    return new List<ConstructorInfo>(new[] { constructor });
                case 2:
                    {
                        if (p[0].ParameterType.IsAssignableFrom(typeof(T)) &&
                            p[1].ParameterType.IsAssignableFrom(typeof(T2)))
                            toReturn.Add(constructor);
                        break;
                    }
            }
        }

        return toReturn;
    }

    /// <summary>
    /// Returns all constructors defined for class 'type' which are compatible with any set or subset of the provided parameters.  The return value is a dictionary
    /// of all compatible constructors with the objects needed to invoke them.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="allowBlankConstructor"></param>
    /// <param name="allowPrivate"></param>
    /// <param name="parameterObjects"></param>
    /// <returns></returns>
    public static Dictionary<ConstructorInfo, List<object>> GetConstructors(Type type, bool allowBlankConstructor,
        bool allowPrivate, params object[] parameterObjects)
    {
        var toReturn = new Dictionary<ConstructorInfo, List<object>>();

        foreach (var constructor in type.GetConstructors(TargetBindingFlags))
        {
            if (constructor.IsPrivate && !allowPrivate)
                continue;

            var p = constructor.GetParameters();

            //if it is a blank constructor
            if (!p.Any())
            {
                if (allowBlankConstructor) //if we do not allow blank constructors ignore it
                    toReturn.Add(constructor,
                        new List<object>()); //otherwise add it to the return list with no objects for invoking (because it's blank duh!)
            }
            else
            {
                //ok we found a constructor that takes some arguments

                //do we have clear 1 to 1 winners on what object to drop into which parameter of the constructor?
                var canInvoke = true;
                var invokeWithObjects = new List<object>();

                //for each object in the constructor
                foreach (var arg in p)
                {
                    //what object could we populate it with?
                    var o = GetBestObjectForPopulating(arg.ParameterType, parameterObjects);

                    //no matching ones sadly
                    if (o == null)
                        canInvoke = false;
                    else
                        invokeWithObjects.Add(o);
                }

                if (canInvoke)
                    toReturn.Add(constructor, invokeWithObjects);
            }
        }

        return toReturn;
    }

    /// <summary>
    /// Returns the best object from parameterObjects for populating an argument of the provided Type.  This is done by looking for an exact Type match first
    /// then if none of those exist, it will look for a single object assignable to the parameter type.  If at any point there is two or more matching parameterObjects
    /// then an <seealso cref="ObjectLacksCompatibleConstructorException"/> will be thrown.
    /// 
    /// <para>If there are no objects provided that match any of the provided parameterObjects then null gets returned.</para>
    /// </summary>
    /// <param name="parameterType"></param>
    /// <param name="parameterObjects"></param>
    /// <returns></returns>
    private static object GetBestObjectForPopulating(Type parameterType, params object[] parameterObjects)
    {
        var matches = parameterObjects.Where(p => p.GetType() == parameterType).ToArray();

        //if there are no exact matches look for an assignable one
        if (matches.Length == 0)
            //look for an assignable one instead
            matches = parameterObjects.Where(parameterType.IsInstanceOfType).ToArray();

        return matches.Length switch
        {
            //if there is one exact match on Type, use that to hydrate it
            1 => matches[0],
            0 => null,
            _ => throw new ObjectLacksCompatibleConstructorException(
                $"Could not pick a suitable parameterObject for populating {parameterType} (found {matches.Length} compatible parameter objects)")
        };
    }

    private static object InvokeBestConstructor(List<ConstructorInfo> constructors, params object[] parameters)
    {
        if (constructors.Count == 1)
            return constructors[0].Invoke(parameters);

        var importDecorated = constructors.Where(c => Attribute.IsDefined(c, typeof(UseWithObjectConstructorAttribute)))
            .ToArray();
        return importDecorated.Length == 1
            ? importDecorated[0].Invoke(parameters)
            : throw new ObjectLacksCompatibleConstructorException(
                $"Could not pick the correct constructor between:{Environment.NewLine}{string.Join($"{Environment.NewLine}", constructors.Select(c => $"{c.Name}({string.Join(",", c.GetParameters().Select(p => p.ParameterType))}"))}");
    }

    private static object GetUsingBlankConstructor(Type t)
    {
        var blankConstructor = t.GetConstructor(Type.EmptyTypes) ??
                               throw new ObjectLacksCompatibleConstructorException(
                                   $"Type '{t}' did not contain a blank constructor");
        return blankConstructor.Invoke(Array.Empty<object>());
    }

    /// <summary>
    /// Attempts to construct an instance of Type typeToConstruct using the provided constructorValues.  This must match on parameter number but ignores order
    /// so if you pass new Obj1(),new Obj2() it could invoke either MyClass(Obj1 a,Obj2 b) or MyClass(Obj2 a, Obj1 b).
    /// <para>Throws <see cref="ObjectLacksCompatibleConstructorException"/> if there are multiple constructors that match the constructorValues</para>
    /// 
    /// <para>Does not invoke the default constructor unless you leave constructorValues blank</para>
    /// <para>returns null if no compatible constructor is found</para>
    /// </summary>
    /// <param name="typeToConstruct"></param>
    /// <param name="constructorValues"></param>
    /// <returns></returns>
    public static object ConstructIfPossible(Type typeToConstruct, params object[] constructorValues)
    {
        var compatible = new List<ConstructorInfo>();

        foreach (var constructor in typeToConstruct.GetConstructors(TargetBindingFlags))
        {
            var p = constructor.GetParameters();

            //must have the same length of arguments as expected
            if (p.Length != constructorValues.Length)
                continue;

            var isCompatible = true;

            for (var index = 0; index < constructorValues.Length; index++)
                //if we have been given a null value for this parameter
                if (constructorValues[index] == null)
                {
                    //if the parameter is value type null is not ok otherwise it is
                    if (p[index].ParameterType.IsEnum || p[index].ParameterType.IsValueType)
                        isCompatible = false;
                }
                else if (!p[index].ParameterType.IsInstanceOfType(constructorValues[index]))
                {
                    isCompatible = false;
                }

            if (isCompatible)
                compatible.Add(constructor);
        }

        return compatible.Any() ? InvokeBestConstructor(compatible, constructorValues) : null;
    }

    /// <summary>
    /// Returns the most likely constructor for creating new instances of a <see cref="DatabaseEntity"/> class e.g.
    /// for <see cref="Catalogue"/> it would return the constructor <see cref="Catalogue(ICatalogueRepository,string)"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static ConstructorInfo GetRepositoryConstructor(Type type)
    {
        var compatible = type.GetConstructors()
            .Select(static constructorInfo => new { constructorInfo, parameters = constructorInfo.GetParameters() })
            .Where(static t => t.parameters.All(static p => p.GetType() != typeof(ShareManager) && p.GetType() != typeof(DbDataReader)) && t.parameters.Any(static p => typeof(IRepository).IsAssignableFrom(p.ParameterType)))
            .Select(static t => t.constructorInfo).ToList();

        return compatible.Count == 1
            ? compatible.Single()
            : throw new ObjectLacksCompatibleConstructorException(
                $"No best constructor found for Type {type} (found {compatible.Count})");
    }
}