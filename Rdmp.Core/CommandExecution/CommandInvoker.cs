// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution
{
    public class CommandInvoker
    {
        private readonly IBasicActivateItems _basicActivator;
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        
        /// <summary>
        /// Delegates provided by <see cref="_basicActivator"/> for fulfilling constructor arguments of the key Type
        /// </summary>
        private List<KeyValuePair<Type, Func<RequiredArgument,object>>> _argumentDelegates;

        /// <summary>
        /// Called when the user attempts to run a command marked <see cref="ICommandExecution.IsImpossible"/>
        /// </summary>
        public event EventHandler<CommandEventArgs> CommandImpossible;
        
        /// <summary>
        /// Called when a command completes successfully
        /// </summary>
        public event EventHandler<CommandEventArgs> CommandCompleted;

        public CommandInvoker(IBasicActivateItems basicActivator)
        {
            _basicActivator = basicActivator;
            _repositoryLocator = basicActivator.RepositoryLocator;

            _argumentDelegates = _basicActivator.GetDelegates();
            
            AddDelegate(typeof(ICatalogueRepository),(p)=>_repositoryLocator.CatalogueRepository);
            AddDelegate(typeof(IDataExportRepository),(p)=>_repositoryLocator.DataExportRepository);
            AddDelegate(typeof(IBasicActivateItems),(p)=>_basicActivator);
            AddDelegate(typeof(IRDMPPlatformRepositoryServiceLocator),(p)=>_repositoryLocator);
            AddDelegate(typeof(DirectoryInfo), (p) => _basicActivator.SelectDirectory($"Enter Directory For Parameter '{p}'"));
            AddDelegate(typeof(FileInfo), (p) => _basicActivator.SelectFile($"Enter File For Parameter '{p}'"));

            AddDelegate(typeof(string), (p) =>
                _basicActivator.TypeText("Value needed for parameter", p.Name, 1000, null, out string result, false)
                ? result
                : null);

            AddDelegate(typeof(Type), (p) =>
            
                _basicActivator.TypeText("Enter Type for parameter", p.Name, 1000, null, out string result, false)
                    ?_repositoryLocator.CatalogueRepository.MEF.GetType(result)
                    :null
            );

            AddDelegate(typeof(DiscoveredDatabase),(p)=>_basicActivator.SelectDatabase(true,"Value needed for parameter " + p.Name));
            AddDelegate(typeof(DiscoveredTable),(p)=>_basicActivator.SelectTable(true,"Value needed for parameter " + p.Name));

            AddDelegate(typeof(DatabaseEntity), (p) =>_basicActivator.SelectOne(p.Name, GetAllObjectsOfType(p.Type)));
            AddDelegate(typeof(IMightBeDeprecated), SelectOne<IMightBeDeprecated>);
            AddDelegate(typeof(IDisableable), SelectOne<IDisableable>);
            AddDelegate(typeof(INamed), SelectOne<INamed>);
            AddDelegate(typeof(IDeleteable), SelectOne<IDeleteable>);

            AddDelegate(typeof(Enum),(p)=>_basicActivator.SelectEnum("Value needed for parameter " + p.Name , p.Type, out Enum chosen)?chosen:null);

            AddDelegate(typeof(ICheckable), 
                (p)=>_basicActivator.SelectOne(p.Name, 
                    _basicActivator.GetAll<ICheckable>()
                        .Where(p.Type.IsInstanceOfType)
                        .Cast<IMapsDirectlyToDatabaseTable>()
                        .ToArray())
                );
        }

        private void AddDelegate(Type type, Func<RequiredArgument, object> func)
        {
            _argumentDelegates.Add(new KeyValuePair<Type, Func<RequiredArgument, object>>(type,func));
        }

        
        public IEnumerable<Type> GetSupportedCommands()
        {
            
            return _basicActivator.RepositoryLocator.CatalogueRepository.MEF.GetAllTypes().Where(IsSupported);
        }

        /// <summary>
        /// Constructs an instance of the <see cref="IAtomicCommand"/> and executes it.  Constructor parameters
        /// are populated from the (optional) <paramref name="picker"/> or the <see cref="IBasicActivateItems"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="picker"></param>
        public void ExecuteCommand(Type type, CommandLineObjectPicker picker)
        {
            ExecuteCommand(GetConstructor(type),picker);
        }
        private void ExecuteCommand(ConstructorInfo constructorInfo, CommandLineObjectPicker picker)
        {
            List<object> parameterValues = new List<object>();
            
            int idx = 0;

            foreach (var parameterInfo in constructorInfo.GetParameters())
            {
                object value = null;

                //if we have argument values specified
                if (picker != null)
                {
                    //and the specified value matches the expected parameter type
                    if (picker.HasArgumentOfType(idx, parameterInfo.ParameterType))
                    {
                        //consume a value
                        value = picker[idx].GetValueForParameterOfType(parameterInfo.ParameterType);
                        idx++;
                    }
                }
                
                if(value == null) 
                    value = GetValueForParameterOfType(parameterInfo);
                
                //if it's a null and not a default null
                if(value == null && !parameterInfo.HasDefaultValue)
                    throw new OperationCanceledException("Could not figure out a value for property '" + parameterInfo + "' for constructor '" + constructorInfo + "'.  Parameter Type was '" + parameterInfo.ParameterType + "'");

                parameterValues.Add(value);
            }
            if(picker != null && idx < picker.Length)
                throw new Exception("Unrecognised extra parameter " + picker[idx].RawValue);

            var instance = (IAtomicCommand)constructorInfo.Invoke(parameterValues.ToArray());
        
            if (instance.IsImpossible)
            {
                CommandImpossible?.Invoke(this,new CommandEventArgs(instance));
                return;
            }

            instance.Execute();
            CommandCompleted?.Invoke(this,new CommandEventArgs(instance));
        }

        public object GetValueForParameterOfType(PropertyInfo propertyInfo)
        {
            return GetValueFor(new RequiredArgument(propertyInfo));
        }

        public object GetValueForParameterOfType(ParameterInfo parameterInfo)
        {
            return GetValueFor(new RequiredArgument(parameterInfo));
        }

        private object GetValueFor(RequiredArgument a)
        {
            var record = _argumentDelegates.Where(p => p.Key.IsAssignableFrom(a.Type))
                .Select(p => new {p.Key, p.Value })
                .FirstOrDefault();

            if (record != null)
                return record.Value(a);
            
            //it's an array of DatabaseEntities
            if(a.Type.IsArray && typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(a.Type.GetElementType()))
            {
                IMapsDirectlyToDatabaseTable[] available = GetAllObjectsOfType(a.Type.GetElementType());
                return _basicActivator.SelectMany(a.Name,a.Type.GetElementType(), available);
            }
            
            if (a.HasDefaultValue)
                return a.DefaultValue;

            if (a.Type.IsValueType && !typeof(Enum).IsAssignableFrom(a.Type))
                return _basicActivator.SelectValueType(a.Name,a.Type);

            return null;
        }

        private T SelectOne<T>(RequiredArgument parameterInfo)
        {
            return (T)_basicActivator.SelectOne(parameterInfo.Name,_basicActivator.GetAll<T>().Cast<IMapsDirectlyToDatabaseTable>().ToArray());
        }

        public bool IsSupported(ConstructorInfo c)
        {
            var notSupported = c.GetParameters().Where(p=>!IsSupported(p));

            return c.GetParameters().All(IsSupported);
        }

        private bool IsSupported(ParameterInfo p)
        {
            return _argumentDelegates.Any(k => k.Key.IsAssignableFrom(p.ParameterType)) ||
                   p.ParameterType.IsArray &&
                   typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(p.ParameterType.GetElementType()) ||
                   p.HasDefaultValue ||
                   p.ParameterType == typeof(string) ||
                   (p.ParameterType.IsValueType && !typeof(Enum).IsAssignableFrom(p.ParameterType));
        }

        public bool IsSupported(Type t)
        {
            bool acceptableType = typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;

            if (!acceptableType)
                return false;

            if (_basicActivator.GetIgnoredCommands().Contains(t))
                return false;

            try
            {
                var constructor = GetConstructor(t);

                if (constructor == null)
                    return false;

                return IsSupported(constructor);

            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors();

            if (constructors.Length == 0)
                return null;

            var importDecorated = constructors.Where(c => Attribute.IsDefined(c, typeof(UseWithObjectConstructorAttribute))).ToArray();

            if (importDecorated.Any())
                return importDecorated[0];

            return constructors[0];
        }
        private IMapsDirectlyToDatabaseTable[] GetAllObjectsOfType(Type type)
        {
            if (type.IsAbstract || type.IsInterface)
                return _basicActivator.GetAll(type).Cast<IMapsDirectlyToDatabaseTable>().ToArray();

            if (_repositoryLocator.CatalogueRepository.SupportsObjectType(type))
                return  _repositoryLocator.CatalogueRepository.GetAllObjects(type).ToArray();
            if (_repositoryLocator.DataExportRepository.SupportsObjectType(type))
                return _repositoryLocator.DataExportRepository.GetAllObjects(type).ToArray();
            
            return null;
        }
    }
}
