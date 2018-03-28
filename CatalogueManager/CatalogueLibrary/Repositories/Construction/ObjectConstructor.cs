using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Repositories.Construction.Exceptions;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories.Construction
{
    /// <summary>
    /// Simplifies identifying and invoking ConstructorInfos on Types (reflection).  This includes identifying a suitable Constructor on a class Type based on the
    /// provided parameters and invoking it.  Also implicitly supports hypotheticals e.g. 'heres a TableInfo, construct class X with the TableInfo paramter or if 
    /// it has a blank constructor that's fine too or if it takes ITableInfo that's fine too... just use whatever works'.  If there are multiple matching constructors
    /// it will attempt to find the 'best' (See InvokeBestConstructor for implementation).
    /// 
    /// If there are no compatible constructors you will get an ObjectLacksCompatibleConstructorException.
    /// </summary>
    public class ObjectConstructor
    {
        private readonly static BindingFlags BindingFlags = BindingFlags.Instance  | BindingFlags.Public| BindingFlags.NonPublic;

        public object Construct(Type t)
        {
            return GetUsingBlankConstructor(t);
        }

        #region permissable constructor signatures for use with this class
        public object Construct(Type t, IRDMPPlatformRepositoryServiceLocator serviceLocator,bool allowBlank = true)
        {
            return Construct<IRDMPPlatformRepositoryServiceLocator>(t,serviceLocator, allowBlank);
        }

        public object Construct(Type t, ICatalogueRepository catalogueRepository, bool allowBlank = true)
        {
            return Construct<ICatalogueRepository>(t, catalogueRepository, allowBlank);
        }


        public IMapsDirectlyToDatabaseTable ConstructIMapsDirectlyToDatabaseObject<T>(Type objectType, T repositoryOfTypeT, DbDataReader reader) where T : IRepository
        {
            // Preferred constructor
            var constructors = GetConstructors<T, DbDataReader>(objectType);

           if (!constructors.Any())
           {
                // Fallback constructor
                throw new ObjectLacksCompatibleConstructorException(objectType.Name + " requires a constructor ("+typeof(T).Name+" repo, DbDataReader reader) to be used with ConstructIMapsDirectlyToDatabaseObject");
            }

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
        public object Construct<T>(Type typeToConstruct, T constructorParameter1, bool allowBlank = true)
        {
            List<ConstructorInfo> repositoryLocatorConstructorInfos = GetConstructors<T>(typeToConstruct);

            if (!repositoryLocatorConstructorInfos.Any())
                if (allowBlank)
                    try
                    {
                        return GetUsingBlankConstructor(typeToConstruct);
                    }
                    catch (ObjectLacksCompatibleConstructorException)
                    {
                        throw new ObjectLacksCompatibleConstructorException("Type '" + typeToConstruct +
                                                                            "' does not have a constructor taking an " +
                                                                            typeof (T) +
                                                                            " - it doesn't even have a blank constructor!");
                    }
                else
                    throw new ObjectLacksCompatibleConstructorException("Type '" + typeToConstruct +
                                                                        "' does not have a constructor taking an " +
                                                                        typeof (T));


            return InvokeBestConstructor(repositoryLocatorConstructorInfos, constructorParameter1);
        }
        
        private List<ConstructorInfo> GetConstructors<T>(Type type)
        {
            var toReturn = new List<ConstructorInfo>();
            ConstructorInfo exactMatch = null;

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags))
            {
                var p = constructor.GetParameters();

                if (p.Length == 1)
                    if (p[0].ParameterType == typeof (T))//is it an exact match i.e. ctor(T bob) 
                        exactMatch = constructor;
                    else
                        if(p[0].ParameterType.IsAssignableFrom(typeof(T))) //is it a derrived class match i.e. ctor(F bob) where F is a derrived class of T
                            toReturn.Add(constructor);
            }

            if(exactMatch != null)
                return new List<ConstructorInfo>(new []{exactMatch});

            return toReturn;
        }


        /// <summary>
        /// Returns all constructors defined for class 'type' that are compatible with the parameters T and T2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<ConstructorInfo> GetConstructors<T,T2>(Type type)
        {
            var toReturn = new List<ConstructorInfo>();
            ConstructorInfo exactMatch = null;

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags))
            {
                var p = constructor.GetParameters();

                if (p.Length == 2)
                    if (p[0].ParameterType == typeof (T) && p[1].ParameterType == typeof (T2))
                        exactMatch = constructor;
                    else
                        if(p[0].ParameterType.IsAssignableFrom(typeof(T)) && p[1].ParameterType.IsAssignableFrom(typeof(T2)))
                            toReturn.Add(constructor);
            }

            if (exactMatch != null)
                return new List<ConstructorInfo>(new[] { exactMatch });

            return toReturn;
        }
        /// <summary>
        /// Returns all constructors defined for class 'type' which are compatible with any set or subset of the provided parameters.  The return value is a dictionary
        /// of all compatible constructors with the objects needed to invoke them.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Dictionary<ConstructorInfo, List<object>> GetConstructors(Type type, bool allowBlankConstructor, bool allowPrivate, params object[] parameterObjects)
        {
            Dictionary<ConstructorInfo,List<object>> toReturn = new Dictionary<ConstructorInfo, List<object>>();

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags))
            {
                if(constructor.IsPrivate && !allowPrivate)
                    continue;

                var p = constructor.GetParameters();

                //if it is a blank constructor
                if(!p.Any())
                    if (!allowBlankConstructor) //if we do not allow blank constructors ignore it
                        continue;
                    else
                        toReturn.Add(constructor, new List<object>()); //otherwise add it to the return list with no objects for invoking (because it's blank duh!)
                else
                {
                    //ok we found a constructor that takes some arguments

                    //do we have clear 1 to 1 winners on what object to drop into which parameter of the constructor?
                    bool canInvoke = true;
                    List<object> invokeWithObjects = new List<object>();

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

                    if(canInvoke)
                        toReturn.Add(constructor,invokeWithObjects);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns the best object from parameterObjects for populating an argument of the provided Type.  This is done by looking for an exact Type match first
        /// then if none of those exist, it will look for a single object assignable to the parameter type.  If at any point there is two or more matching parameterObjects
        /// then an <seealso cref="ObjectLacksCompatibleConstructorException"/> will be thrown.
        /// 
        /// If there are no objects provided that match any of the provided parameterObjects then null gets returned.
        /// </summary>
        /// <param name="parameterType"></param>
        /// <param name="parameterObjects"></param>
        /// <returns></returns>
        private object GetBestObjectForPopulating(Type parameterType, params object[] parameterObjects)
        {
            var matches = parameterObjects.Where(p => p.GetType() == parameterType).ToArray();

            //if there are no exact matches look for an assignable one
            if (matches.Length == 0)
            {
                //look for an assignable one instead
                matches = parameterObjects.Where(parameterType.IsInstanceOfType).ToArray();
            }
            
            //if there is one exact match on Type, use that to hydrate it
            if (matches.Length == 1)
                return matches[0];

            if (matches.Length == 0)
                return null;

            throw new ObjectLacksCompatibleConstructorException("Could not pick a suitable parameterObject for populating " + parameterType + " (found " + matches.Length + " compatible parameter objects)");

        }

        private object InvokeBestConstructor(List<ConstructorInfo> constructors, params object[] parameters)
        {
            if (constructors.Count == 1)
                return constructors[0].Invoke(parameters);

            var importDecorated = constructors.Where(c => Attribute.IsDefined(c, typeof (ImportingConstructorAttribute))).ToArray();
            if(importDecorated.Length == 1)
                return importDecorated[0].Invoke( parameters);

            throw new ObjectLacksCompatibleConstructorException("Could not pick the correct constructor between:" + Environment.NewLine
                + string.Join(""+Environment.NewLine,constructors.Select(c=>c.Name +"(" + string.Join(",",c.GetParameters().Select(p=>p.ParameterType)))));
        }

        private object GetUsingBlankConstructor(Type t)
        {
            var blankConstructor = t.GetConstructor(Type.EmptyTypes);

            if (blankConstructor == null)
                throw new ObjectLacksCompatibleConstructorException("Type '" + t + "' did not contain a blank constructor");

            return (blankConstructor.Invoke(new object[0]));
        }

        public bool HasBlankConstructor(Type arg)
        {
            return arg.GetConstructor(Type.EmptyTypes) != null;
        }

        public object ConstructIfPossible(Type typeToConstruct, params object[] constructorValues)
        {
            List<ConstructorInfo> compatible = new List<ConstructorInfo>();

            foreach (var constructor in typeToConstruct.GetConstructors(BindingFlags))
            {
                var p = constructor.GetParameters();

                //must have the same length of arguments as expected
                if (p.Length != constructorValues.Length)
                    continue;

                bool isCompatible = true;

                for (int index = 0; index < constructorValues.Length; index++)
                {
                    if (!p[index].ParameterType.IsInstanceOfType(constructorValues[index]))
                        isCompatible = false;
                }

                if(isCompatible)
                    compatible.Add(constructor);
            }

            if (compatible.Any())
                return InvokeBestConstructor(compatible,constructorValues);

            return null;
        }

    }
}
