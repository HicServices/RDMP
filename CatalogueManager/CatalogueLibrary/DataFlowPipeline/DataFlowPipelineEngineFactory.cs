using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    public class DataFlowPipelineEngineFactory<T> : IDataFlowPipelineEngineFactory
    {
        private readonly MEF _mefPlugins;
        private readonly DataFlowPipelineContext<T> _context;

        public IDataFlowSource<T> ExplicitSource { get; set; }
        public IDataFlowDestination<T> ExplicitDestination { get; set; } 

        public DataFlowPipelineContext<T> Context
        {
            get { return _context; }
        }

        public DataFlowPipelineEngineFactory(MEF mefPlugins, DataFlowPipelineContext<T> context)
        {
            _mefPlugins = mefPlugins;
            _context = context;
        }

        public IDataFlowPipelineEngine Create(IPipeline pipeline, IDataLoadEventListener listener)
        {
            string reason;

            if (!_context.IsAllowable(pipeline, out reason))
                throw new Exception("Cannot create pipeline because: " + reason);

            var destination = GetBest(ExplicitDestination, CreateDestinationIfExists(pipeline),"destination");
            var source = GetBest(ExplicitSource, CreateSourceIfExists(pipeline),"source");
            
            //engine (this is the source, target is the destination)
            DataFlowPipelineEngine<T> dataFlowEngine = new DataFlowPipelineEngine<T>(_context, source, destination, listener);

            //now go fetch everything that the user has configured for this particular pipeline
            foreach (PipelineComponent toBuild in pipeline.PipelineComponents)
            {
                //if it is the destination do not add it
                if (toBuild.ID == pipeline.DestinationPipelineComponent_ID)
                    continue;

                //if it is the source do not add it
                if (toBuild.ID == pipeline.SourcePipelineComponent_ID)
                    continue;
                
                //get the factory to realize the freaky Export types defined in any assembly anywhere and set their DemandsInitialization properties based on the Arguments
                IDataFlowComponent<T> component = CreateComponent(toBuild);
                
                //Add the components to the pipeline
                dataFlowEngine.Components.Add(component);
            }

            return dataFlowEngine;
        }

        /// <summary>
        /// Returns the thing that is not null or throws an exception because both are blank.  also throws if both are populated
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="explicitThing"></param>
        /// <param name="pipelineConfigurationThing"></param>
        /// <param name="descriptionOfWhatThingIs"></param>
        /// <returns></returns>
        private T2 GetBest<T2>(T2 explicitThing, T2 pipelineConfigurationThing, string descriptionOfWhatThingIs)
        {
            // if explicitThing and pipelineConfigurationThing are both null
            //Means: xplicitThing == null && pipelineConfigurationThing == null
            if (EqualityComparer<T2>.Default.Equals(explicitThing, default(T2)) && EqualityComparer<T2>.Default.Equals(pipelineConfigurationThing, default(T2)))
                throw new Exception("No explicit " + descriptionOfWhatThingIs + " was specified and there is no fixed " + descriptionOfWhatThingIs + " defined in the Pipeline configuration in the Catalogue");

            //if one of them only is null - XOR
            if(EqualityComparer<T2>.Default.Equals(explicitThing, default(T2)) ^ EqualityComparer<T2>.Default.Equals(pipelineConfigurationThing, default(T2)))
                return EqualityComparer<T2>.Default.Equals(explicitThing, default(T2)) ? pipelineConfigurationThing : explicitThing; //return the not null one

            //both of them are populated
            throw new Exception("Cannot have both the explicit " + descriptionOfWhatThingIs + " '" + explicitThing + "' (the code creating the pipeline said it had a specific " + descriptionOfWhatThingIs + " it wants to use) as well as the " + descriptionOfWhatThingIs + " configured in the Pipeline in the Catalogue '" + pipelineConfigurationThing + "' (this should have been picked up by the DataFlowPipelineContext checks above)");
        }


        public object TryCreateComponent(IPipeline pipeline, IPipelineComponent component, out Exception ex)
        {
            ex = null;
            try
            {
                if (component.ID == pipeline.SourcePipelineComponent_ID)
                    return CreateSourceComponent(component);

                return CreateComponent(component);
            }
            catch (Exception e)
            {
                ex = e;
                return null;
            }
        }


        private IDataFlowSource<T> CreateSourceComponent(IPipelineComponent toBuild)
        {
            return CreateComponent<IDataFlowSource<T>>(toBuild);
        }
        private IDataFlowComponent<T> CreateComponent(IPipelineComponent toBuild)
        {
            return CreateComponent<IDataFlowComponent<T>>(toBuild);
        }
        private T2 CreateComponent<T2>(IPipelineComponent toBuild)
        {
            T2 toReturn = _mefPlugins.FactoryCreateA<T2>(toBuild.Class);

            //all the IArguments we need to initialize the class
            var allArguments = toBuild.GetAllArguments().ToArray();

            //get all possible properties that we could set on the underlying class
            foreach (var propertyInfo in toReturn.GetType().GetProperties())
            {
                //see if any demand initialization
                Attribute initialization =
                    System.Attribute.GetCustomAttributes(propertyInfo)
                        .FirstOrDefault(a => a is DemandsInitializationAttribute);

                //this one does
                if (initialization != null)
                {
                    try
                    {
                        //get the appropriate value from arguments
                        var value = allArguments.SingleOrDefault(n => n.Name.Equals(propertyInfo.Name));

                        if (value == null)
                            throw new Exception("Class " + toReturn.GetType().Name + " has a property " + propertyInfo.Name +
                          " marked with DemandsInitialization but no corresponding argument was found in the arguments (PipelineComponentArgument) of the PipelineComponent called " + toBuild.Name);

                        //use reflection to set the value
                        propertyInfo.SetValue(toReturn, value.GetValueAsSystemType(), null);
                    }
                    catch (NotSupportedException e)
                    {
                        throw new Exception("Class " + toReturn.GetType().Name + " has a property " + propertyInfo.Name +
                                            " but is of unexpected/unsupported type " + propertyInfo.GetType(), e);
                    }
                }

                //see if any demand nested initialization
                Attribute nestedInit =
                    System.Attribute.GetCustomAttributes(propertyInfo)
                        .FirstOrDefault(a => a is DemandsNestedInitializationAttribute);

                //this one does
                if (nestedInit != null)
                {
                    // initialise the container before nesting-setting all properties
                    var container = Activator.CreateInstance(propertyInfo.PropertyType);

                    foreach (var nestedProp in propertyInfo.PropertyType.GetProperties())
                    {
                        //see if any demand initialization
                        initialization = System.Attribute.GetCustomAttributes(nestedProp)
                                            .FirstOrDefault(a => a is DemandsInitializationAttribute);

                        //this one does
                        if (initialization != null)
                        {
                            var dottedName = propertyInfo.Name + "." + nestedProp.Name;

                            try
                            {
                                //get the appropriate value from arguments
                                var value = allArguments.SingleOrDefault(n => n.Name.Equals(dottedName));

                                if (value == null)
                                    throw new Exception("Class " + toReturn.GetType().Name + " has a property " +
                                                        dottedName +
                                                        " marked with DemandsNestedInitialization but no corresponding argument was found in the arguments (PipelineComponentArgument) of the PipelineComponent called " +
                                                        toBuild.Name);

                                //use reflection to set the value
                                nestedProp.SetValue(container, value.GetValueAsSystemType(), null);
                            }
                            catch (NotSupportedException e)
                            {
                                throw new Exception(
                                    "Class " + toReturn.GetType().Name + " has a nested property " + dottedName +
                                    " but is of unexpected/unsupported type " + nestedProp.GetType(), e);
                            }
                        }
                    }

                    //use reflection to set the container
                    propertyInfo.SetValue(toReturn, container, null);
                }
            }

            return toReturn;
        }


        public IDataFlowSource<T> CreateSourceIfExists(IPipeline pipeline)
        {
            var source = pipeline.Source;

            //there is no configured destination
            if (source == null)
                return null;

            return CreateSourceComponent(source);
        }

        public IDataFlowDestination<T> CreateDestinationIfExists(IPipeline pipeline)
        {
            var destination = pipeline.Destination;

            //there is no configured destination
            if (destination == null)
                return null;

            IDataFlowComponent<T> toReturn = CreateComponent(destination);

            if (toReturn is IDataFlowDestination<T>)
                return (IDataFlowDestination<T>) toReturn;
            else
                throw new NotSupportedException("The IsDestination PipelineComponent of pipeline '" + pipeline.Name + "' is an IDataFlowComponent but it is not an IDataFlowDestination which is a requirement of all destinations");
        }

        public void Check(IPipeline pipeline, ICheckNotifier checkNotifier, object[] initizationObjects)
        {
            //Try to construct the pipeline into an in memory Engine based on the in Catalogue blueprint (Pipeline)
            IDataFlowPipelineEngine dataFlowPipelineEngine = null;
            try
            {
                dataFlowPipelineEngine = Create(pipeline, new FromCheckNotifierToDataLoadEventListener(checkNotifier));
                checkNotifier.OnCheckPerformed(new CheckEventArgs("Pipeline successfully constructed in memory", CheckResult.Success));
            }
            catch (Exception exception)
            {
                checkNotifier.OnCheckPerformed(
                    new CheckEventArgs("Failed to construct pipeline, see Exception for details", CheckResult.Fail,
                        exception));
            }

            if (initizationObjects == null)
            {
                checkNotifier.OnCheckPerformed(new CheckEventArgs("initizationObjects parameter has not been set (this is a programmer error most likely ask your developer to fix it - this parameter should be empty not null)",
                    CheckResult.Fail));
                return;
            }
            
            //Initialize each component with the initialization objects so that they can check themselves (note that this should be preview data, hopefully the components don't run off and start nuking stuff just because they got their GO objects)
            if(dataFlowPipelineEngine != null)
            {

                try
                {
                    dataFlowPipelineEngine.Initialize(initizationObjects);
                    checkNotifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Pipeline sucesfully initialized with " +
                            initizationObjects.Length + " initialization objects",
                            CheckResult.Success));
                }
                catch (Exception exception)
                {
                    checkNotifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Pipeline initialization failed, there were " + initizationObjects.Length +
                            " objects for use in initialization (" +
                            string.Join(",", initizationObjects.Select(o => o.ToString())) + ")", CheckResult.Fail,
                            exception));
                }

                checkNotifier.OnCheckPerformed(new CheckEventArgs("About to check engine/components", CheckResult.Success));
                dataFlowPipelineEngine.Check(checkNotifier);
            }
        }

      
    }
}
