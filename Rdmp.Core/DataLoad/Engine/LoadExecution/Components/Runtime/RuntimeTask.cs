// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
/// Runtime realisation of a ProcessTask.  ProcessTask is the DesignTime template configured by the user.  RuntimeTask is the 'ready to execute' version.
/// There are two main kinds of RuntimeTask in the RDMP data load engine (See DataLoadEngine.cd).
/// 
/// <para>The first are those that host a class instance (IMEFRuntimeTask) e.g. IAttacher, IDataProvider.  In this case the ProcessTask is expected to be configured
/// with the class Type and have IArguments that specify all values for hydrating the instance created (See ProcessTaskArgument).</para>
/// 
/// <para>The second are those that do not have a MEF class powering them.  This includes ProcessTaskTypes like Executable and SQLFile where the ProcessTask.Path
/// is simply the location of the exe/sql file to run at runtime. </para>
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
            throw new NullReferenceException(
                $"{ProcessTask.Path} instance has not been created yet! Call SetProperties after the factory has created the instance.");

        if (UsefulStuff.IsAssignableToGenericType(toSetPropertiesOf.GetType(), typeof(IPipelineRequirement<>)))
            throw new Exception(
                $"ProcessTask '{ProcessTask.Name}' was was an instance of Class '{ProcessTask.Path}' which declared an IPipelineRequirement<>.  RuntimeTask classes are not the same as IDataFlowComponents, IDataFlowComponents can make IPipelineRequirement requests but RuntimeTasks cannot");

        if (UsefulStuff.IsAssignableToGenericType(toSetPropertiesOf.GetType(), typeof(IPipelineOptionalRequirement<>)))
            throw new Exception(
                $"ProcessTask '{ProcessTask.Name}' was was an instance of Class '{ProcessTask.Path}' which declared an IPipelineOptionalRequirement<>.  RuntimeTask classes are not the same as IDataFlowComponents, IDataFlowComponents can make IPipelineRequirement requests but RuntimeTasks cannot");


        //get all possible properties that we could set
        foreach (var propertyInfo in toSetPropertiesOf.GetType().GetProperties())
        {
            //see if any demand initialization
            var initialization = (DemandsInitializationAttribute)Attribute.GetCustomAttributes(propertyInfo)
                .FirstOrDefault(static a => a is DemandsInitializationAttribute);

            //this one does
            if (initialization == null) continue;

            try
            {
                //get the approrpriate value from arguments
                var value = args.GetCustomArgumentValue(propertyInfo.Name);

                //use reflection to set the value
                propertyInfo.SetValue(toSetPropertiesOf, value, null);
            }
            catch (NotSupportedException e)
            {
                throw new Exception(
                    $"Class {toSetPropertiesOf.GetType().Name} has a property {propertyInfo.Name} but is of unexpected type {propertyInfo.GetType()}",
                    e);
            }
            catch (KeyNotFoundException e)
            {
                if (initialization.Mandatory)
                    throw new ArgumentException(
                        $"Class {toSetPropertiesOf.GetType().Name} has a Mandatory property '{propertyInfo.Name}' marked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection",
                        e);
            }
        }
    }

    public abstract void Check(ICheckNotifier checker);
}