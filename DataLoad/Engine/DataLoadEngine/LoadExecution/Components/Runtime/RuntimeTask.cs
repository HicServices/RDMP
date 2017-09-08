using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataLoadEngine.LoadExecution.Components.Arguments;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public abstract class RuntimeTask : DataLoadComponent, IRuntimeTask
    {
        public RuntimeArgumentCollection RuntimeArguments { get; set; }
        
        public int? RelatesSolelyToCatalogue_ID { get; set; }

        public IEnumerable<IArgument> GetAllArguments()
        {
            return RuntimeArguments.Arguments;
        }

        public abstract bool Exists();

        public bool RunContinuously { get; set; }
        public string Path { get; set; }
        public string Name { get; private set; }
        public LoadStage LoadStage { get; private set; }
        public ProcessTaskType ProcessTaskType { get; protected set; }
        public int Order { get; set; }

        public abstract void Abort(IDataLoadEventListener postLoadEventListener);

        protected RuntimeTask(IProcessTask processTask, RuntimeArgumentCollection args)
        {
            RunContinuously = false;
            ProcessTaskType = processTask.ProcessTaskType;
            Path = processTask.Path;
            RuntimeArguments = args;
            Name = processTask.Name;
            LoadStage = processTask.LoadStage;
            Order = processTask.Order;
            RelatesSolelyToCatalogue_ID = processTask.RelatesSolelyToCatalogue_ID;
        }

        public void SetPropertiesForClass(RuntimeArgumentCollection args, object toSetPropertiesOf)
        {
            if (toSetPropertiesOf == null)
                throw new NullReferenceException("Attacher has not been created yet! Call SetProperties after the factory has created the attacher.");
            
            if (UsefulStuff.IsAssignableToGenericType(toSetPropertiesOf.GetType(),typeof (IPipelineRequirement<>)))
                throw new Exception("ProcessTask '" + Name + "' was was an instance of Class '" + Path + "' which declared an IPipelineRequirement<>.  RuntimeTask classes are not the same as IDataFlowComponents, IDataFlowComponents can make IPipelineRequirement requests but RuntimeTasks cannot");

            //get all possible properties that we could set
            foreach (var propertyInfo in toSetPropertiesOf.GetType().GetProperties())
            {
                //see if any demand initialization
                Attribute initialization = System.Attribute.GetCustomAttributes(propertyInfo).FirstOrDefault(a => a is DemandsInitializationAttribute);

                //this one does
                if (initialization != null)
                {
                    try
                    {
                        //get the approrpriate value from arguments
                        var value = args.GetCustomArgumentValue(propertyInfo.Name, propertyInfo.PropertyType);

                        //use reflection to set the value
                        propertyInfo.SetValue(toSetPropertiesOf, value, null);
                    }
                    catch (NotSupportedException e)
                    {
                        throw new Exception("Class " + toSetPropertiesOf.GetType().Name + " has a property " + propertyInfo.Name +
                                            " but is of unexpected type " + propertyInfo.GetType(), e);
                    }
                    catch (KeyNotFoundException e)
                    {
                        throw new ArgumentException(
                          "Class " + toSetPropertiesOf.GetType().Name + " has a property " + propertyInfo.Name +
                          "marked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection", e);
                    }
                }
            }
        }

        public abstract void Check(ICheckNotifier checker);
    }
}