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
    public class ObjectConstructor
    {
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

        public object Construct<T>(Type t, T o, bool allowBlank = true)
        {
            List<ConstructorInfo> repositoryLocatorConstructorInfos = GetConstructors<T>(t);

            if (!repositoryLocatorConstructorInfos.Any())
                if (allowBlank)
                    try
                    {
                        return GetUsingBlankConstructor(t);
                    }
                    catch (ObjectLacksCompatibleConstructorException)
                    {
                        throw new ObjectLacksCompatibleConstructorException("Type '" + t +
                                                                            "' does not have a constructor taking an " +
                                                                            typeof (T) +
                                                                            " - it doesn't even have a blank constructor!");
                    }
                else
                    throw new ObjectLacksCompatibleConstructorException("Type '" + t +
                                                                        "' does not have a constructor taking an " +
                                                                        typeof (T));


            return InvokeBestConstructor(repositoryLocatorConstructorInfos, o);
        }

        private List<ConstructorInfo> GetConstructors<T>(Type type)
        {
            var toReturn = new List<ConstructorInfo>();

            foreach (ConstructorInfo constructor in type.GetConstructors())
            {
                var p = constructor.GetParameters();

                if (p.Length == 1 && typeof(T).IsAssignableFrom(p[0].ParameterType))
                    toReturn.Add(constructor);
            }

            return toReturn;
        }
        private List<ConstructorInfo> GetConstructors<T,T2>(Type type)
        {
            var toReturn = new List<ConstructorInfo>();

            foreach (ConstructorInfo constructor in type.GetConstructors())
            {
                var p = constructor.GetParameters();

                if (p.Length == 2 && typeof(T).IsAssignableFrom(p[0].ParameterType) && typeof(T2).IsAssignableFrom(p[1].ParameterType))
                    toReturn.Add(constructor);
            }

            return toReturn;
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
    }
}
