using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data.DataLoad;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    public class RuntimeArgumentCollection
    {
        public IStageArgs StageSpecificArguments
        {
            get;
            set;
        }

        public HashSet<IArgument> Arguments { get; private set; }

        /// <summary>
        /// Transition from the arguments defined in the data source (Catalogue Database) into a runtime state
        /// </summary>
        public RuntimeArgumentCollection(IArgument[] args, IStageArgs stageSpecificArgs)
        {
            StageSpecificArguments = stageSpecificArgs;
            Arguments = new HashSet<IArgument>();

            if (!args.Any())
                return;

            foreach (var processTaskArgument in args)
                AddArgument(processTaskArgument);
        }

        public void IterateAllArguments(Func<string, object, bool> func)
        {
            foreach (var arg in Arguments)
            {
                func(arg.Name, arg.GetValueAsSystemType());
            }
            
            if (StageSpecificArguments == null)
                return;

            foreach (var arg in StageSpecificArguments.ToDictionary())
                func(arg.Key, arg.Value);
        }

        public void AddArgument(IArgument processTaskArgument)
        {
            Arguments.Add(processTaskArgument);
        }

        public object GetCustomArgumentValue(string name, Type type)
        {
            IArgument first = Arguments.SingleOrDefault(a => a.Name.Equals(name));

            if(first == null)
                throw new KeyNotFoundException("Argument " + name + " was missing");

            try
            {
                return first.GetValueAsSystemType();
            }
            catch (Exception e)
            {
                throw new Exception("Could not convert value '" + first.Value + "' into a " + first.GetSystemType().FullName + " for argument '" + first.Name + "'", e);
            }
        }

        public IEnumerable<KeyValuePair<string,object>>  GetAllArguments()
        {
            if(StageSpecificArguments != null)
                foreach (var kvp in StageSpecificArguments.ToDictionary())
                    yield return kvp;

            foreach (IArgument argument in Arguments)
                yield return new KeyValuePair<string, object>(argument.Name, argument.GetValueAsSystemType());
        }

        public IEnumerable<KeyValuePair<string, T>> GetAllArgumentsOfType<T>()
        {
            if (StageSpecificArguments != null)
                foreach (var kvp in StageSpecificArguments.ToDictionary())
                    if(kvp.Value.GetType() == typeof(T))
                        yield return new KeyValuePair<string, T>(kvp.Key,(T)kvp.Value);

            foreach (IArgument argument in Arguments)
                if(argument.GetSystemType() == typeof(T))
                    yield return new KeyValuePair<string, T>(argument.Name, (T)argument.GetValueAsSystemType());

        }
    }
}
