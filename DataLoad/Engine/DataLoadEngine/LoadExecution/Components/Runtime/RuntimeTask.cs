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
    /// <summary>
    /// Runtime realisation of a ProcessTask.  ProcessTask is the DesignTime template configured by the user.  RuntimeTask is the 'ready to execute' version.
    /// There are two main kinds of RuntimeTask in the RDMP data load engine (See DataLoadEngine.cd).
    /// 
    /// The first are those that host a class instance (IMEFRuntimeTask) e.g. IAttacher, IDataProvider.  In this case the ProcessTask is expected to be configured
    /// with the class Type and have IArguments that specify all values for hydrating the instance created (See ProcessTaskArgument).
    /// 
    /// The second are those that do not have a MEF class powering them.  This includes ProcessTaskTypes like Executable and SQLFile where the ProcessTask.Path
    /// is simply the location of the exe/sql file to run at runtime. 
    /// </summary>
    public abstract class RuntimeTask : DataLoadComponent, IRuntimeTask
    {
        public RuntimeArgumentCollection RuntimeArguments { get; set; }
        public IProcessTask ProcessTask { get; set; }
        
        public abstract bool Exists();

        public abstract void Abort(IDataLoadEventListener postLoadEventListener);

        protected RuntimeTask(IProcessTask processTask, RuntimeArgumentCollection args)
        {
            ProcessTask = processTask;
            RuntimeArguments = args;
        }

        public void SetPropertiesForClass(RuntimeArgumentCollection args, object toSetPropertiesOf)
        {
            if (toSetPropertiesOf == null)
                throw new NullReferenceException(ProcessTask.Path + " instance has not been created yet! Call SetProperties after the factory has created the instance.");
            
            if (UsefulStuff.IsAssignableToGenericType(toSetPropertiesOf.GetType(),typeof (IPipelineRequirement<>)))
                throw new Exception("ProcessTask '" + ProcessTask.Name + "' was was an instance of Class '" + ProcessTask.Path + "' which declared an IPipelineRequirement<>.  RuntimeTask classes are not the same as IDataFlowComponents, IDataFlowComponents can make IPipelineRequirement requests but RuntimeTasks cannot");

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