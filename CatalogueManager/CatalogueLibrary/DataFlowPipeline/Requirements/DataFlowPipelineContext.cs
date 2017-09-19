using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    public delegate void ContextInitialzedObjectEventHandler(object componentBeingInitialized, object valueBeingConsumed);

    public class DataFlowPipelineContext<T>: IDataFlowPipelineContext
    {
        public Type MustHaveSource { get; set; }
        public Type MustHaveDestination { get; set; }
        public HashSet<Type> CannotHave { get; private set; }
        
        public event ContextInitialzedObjectEventHandler ObjectInitialized = delegate { };

        public DataFlowPipelineContext()
        {
            CannotHave = new HashSet<Type>();
        }

        public bool IsAllowable(Type t)
        {
            string whoCares;
            return IsAllowable(t, out whoCares);
        }
        
        public bool IsAllowable(Type t, out string reason)
        {
            //if t is a source
            if (typeof(IDataFlowSource<T>).IsAssignableFrom(t))
                if (MustHaveSource == null) //we do not allow sources!
                {
                    reason = "The current context does not allow for IDataFlowSources";
                    return false;
                }
                else
                {
                    reason = null;
                    var isAssignable = MustHaveSource.IsAssignableFrom(t);
                    if (!isAssignable)
                        reason = "Type " + GetFullName(t) + " is not a " + GetFullName(MustHaveSource) + " (which is the required type for all sources within the current context)";

                    return isAssignable;
                }

            //if t is a destination
            if (typeof(IDataFlowDestination<T>).IsAssignableFrom(t))
                if (MustHaveDestination == null) // we do not allow destinations
                {
                    reason = "The current context does not allow for IDataFlowDestinations";
                    return false;
                }
                else
                {
                    reason = null;
                    var isAssignable = MustHaveDestination.IsAssignableFrom(t);
                    if (!isAssignable)
                        reason = "Type " + GetFullName(t) +  " is not a " + GetFullName(MustHaveDestination) + " (which is the required type for all destinations within the current context)";

                    return isAssignable;
                }


            //if there are any forbidden types further up the inheritance heirarchy than t reject it
            foreach (var forbiddenType in CannotHave)
            {
                if (forbiddenType.IsAssignableFrom(t))
                {
                    reason = "Type " + GetFullName(t) + " is inherited from a type which is forbidden in the current context: " + GetFullName(forbiddenType);
                    return false;
                }

                if (forbiddenType.IsInterface //or forbidden thing is IPipelineRequirement<TableInfo>
                    &&
                    forbiddenType.IsGenericType
                    &&
                    forbiddenType.GenericTypeArguments.Any(subtype => subtype.IsAssignableFrom(t))) //and t is TableInfo (generic <T> == t)
                {
                    reason = "Type " + GetFullName(t) + " is a generic parameter of forbidden type " + GetFullName(forbiddenType) + "(Forbidden within the current context)";
                    return false;
                }

            }

            reason = null;
            return true;
        }

        private bool IsAllowable(Type t, out Type forbiddenType)
        {
            forbiddenType = CannotHave.SingleOrDefault(type => type.IsAssignableFrom(t));

            return forbiddenType == null;
        }

        public bool IsAllowable(IPipeline pipeline, out string reason)
        {
            foreach (var component in pipeline.PipelineComponents)
            {
                Type forbiddenType;

                if (!IsAllowable(component.GetClassAsSystemType(), out forbiddenType))
                {
                    reason = "Component " + component.Name + " implements a forbidden type (" + GetFullName(forbiddenType) + ") under the pipeline usage context";
                    return false;
                }
            }

            reason = MustHave(MustHaveDestination, pipeline.Destination, "destination");
            if (reason != null)
                return false;

            reason = MustHave(MustHaveSource, pipeline.Source, "source");
            return reason == null;


        }

        private string MustHave(Type mustHaveType, IPipelineComponent component, string descriptionOfThingBeingChecked)
        {
            //it must have destination
            if (mustHaveType != null)
                if (component == null)
                    return "An explicit " + descriptionOfThingBeingChecked + " must be chosen";
                else
                {
                    Type pipelineComponentType = component.GetClassAsSystemType();

                    if (pipelineComponentType == null)
                        return "PipelineComponent " + component.Class + " could not be created, check MEF assembly loading in the Diagnostics menu";

                    if (!mustHaveType.IsAssignableFrom(pipelineComponentType))
                    {
                        return "The pipeline requires a " + descriptionOfThingBeingChecked + " of type " + GetFullName(mustHaveType) +
                                 " but the currently configured " + descriptionOfThingBeingChecked + GetFullName(pipelineComponentType) +
                                 " is not of the same type or a derrived type";
                    }
                }
            else
             //it cannot have destination 
                if (component != null)
                    return "Context does not allow for an explicit (custom) " + descriptionOfThingBeingChecked;


            return null;
        }

        public bool IsAllowable(IPipeline pipeline)
        {
            string whocares;
            return IsAllowable(pipeline, out whocares);
        }

        //objects that we can initialize with IPipelineRequirement<>
        public void PreInitialize(IDataLoadEventListener listener, IDataFlowComponent<T> component, params object[] parameters)
        {
            PreInitializeComponentWithAllObjects(listener, component, parameters);
        }
        public void PreInitialize(IDataLoadEventListener listener, IDataFlowSource<T> component, params object[] parameters)
        {
            PreInitializeComponentWithAllObjects(listener, component, parameters);
        }
        
        protected void PreInitializeComponentWithAllObjects(IDataLoadEventListener listener, object component, params object[] parameters)
        {
            Dictionary<object, Dictionary<MethodInfo, object>> initializedComponents = new Dictionary<object, Dictionary<MethodInfo, object>>();

            //these are all the interfaces like IPipelineRequirement<TableInfo> etc
            var requirements = component.GetType().GetInterfaces().Where(i =>
                i.IsGenericType && 
                i.GetGenericTypeDefinition() 
                == typeof(IPipelineRequirement<>)).ToArray();

            //these are the T tokens in the above interfaces
            var typesRequired = requirements.Select(i => i.GenericTypeArguments[0]).ToList();

            // Check if we have some PreInitialize functions for which there are no IPipelineRequirements, most likely an oversight one way or the other
            var preInitializeFunctions = component.GetType().GetMethods().Where(mi => mi.Name == "PreInitialize");
            var preInitializeTypes = preInitializeFunctions.Select(mi => mi.GetParameters()[0].ParameterType);
            var typesWithNoInterface = preInitializeTypes.Except(typesRequired).ToList();
            if (typesWithNoInterface.Any())
                throw new InvalidOperationException("Found PreInitialize functions in component " + component.GetType().Name + " (or parent types) with no corresponding interface declaration for types: " + string.Join(", ", typesWithNoInterface.Select(t => t.Name)));
           
            //now initialize all the parameters
            foreach (object parameter in parameters)
            {
                if (parameter == null)
                    throw new InvalidOperationException("One of the parameters for PreInitialization of " + component + " is null");

                //see if any of them are forbidden (e.g. if context was created without LoadsSingleTableInfo then it is forbidden to have components with IPipelineRequirement<TableInfo>)
                if(!IsAllowable(parameter.GetType()))
                    throw new Exception("Type " + GetFullName(parameter.GetType()) + " is not an allowable PreInitialize parameters type under the current DataFlowPipelineContext (check which flags you passed to the DataFlowPipelineContextFactory and the interfaces IPipelineRequirement<> that your components implement) ");

                var toRemove = PreInitializeComponentWithSingleObject(listener, component, parameter, initializedComponents);
                
                if (toRemove != null)
                    typesRequired.Remove(toRemove);
            }

            if(typesRequired.Any())
                throw new Exception(
                    "The following expected types were not passed to PreInitialize:" + string.Join(",",typesRequired.Select(GetFullName))
                    
                    +Environment.NewLine + "The object types passed were:" +Environment.NewLine +
                    string.Join(Environment.NewLine,parameters.Select(p=>p.GetType() + ":" + p.ToString()))
                    );
        }

        protected Type PreInitializeComponentWithSingleObject(IDataLoadEventListener listener, object component, object value, Dictionary<object, Dictionary<MethodInfo, object>> initializedComponents)
        {
            var compatibleInterfaces = component.GetType()
                .GetInterfaces().Where(i => 
                    i.IsGenericType && (i.GenericTypeArguments[0] == value.GetType() || i.GenericTypeArguments[0].IsInstanceOfType(value))
                    ).ToArray();

            if (compatibleInterfaces.Length == 0)
                return null;

            if(compatibleInterfaces.Length > 1)
                throw new OverlappingImplementationsException("The following IPipelineRequirement<> interfaces are implemented on pipeline component of type '" + component.GetType().Name + "' which are intercompatible with the input object of type '" + value.GetType().Name +"' "+ string.Join(",", compatibleInterfaces.Select(GetFullName)));

            var interfaceToInvokeIfAny = compatibleInterfaces[0];
            if (interfaceToInvokeIfAny != null)
            {
                //We have an interface that matches the input object, let's call it
                var preInit = interfaceToInvokeIfAny.GetMethod("PreInitialize");
                
                //but first document the fact that we have foundit
                if (!initializedComponents.ContainsKey(component))
                    initializedComponents.Add(component, new Dictionary<MethodInfo, object>());

                if (initializedComponents[component].ContainsKey(preInit))
                    throw new MultipleMatchingImplmentationException("Interface " + GetFullName(interfaceToInvokeIfAny) + " matches both input objects '" + initializedComponents[component][preInit] + "' ('" + initializedComponents[component][preInit].GetType().Name + "') and '" + value + "' ('" + value.GetType().Name + "')");
                else
                    initializedComponents[component].Add(preInit, value);
                
                //invoke it
                preInit.Invoke(component, new[] {value, listener});

                //call the event that lets external viewers know we called it
                ObjectInitialized(component, value);

                //return the type of T for IPipelineRequirement<T> interface that was called
                return interfaceToInvokeIfAny.GenericTypeArguments[0];
            }

            return null;
        }

        public void PreInitializeGeneric(IDataLoadEventListener listener, object component, params object[] initializationObjects)
        {
            if(component is IDataFlowSource<T>)
                PreInitialize(listener, (IDataFlowSource<T>)component, initializationObjects);
            else if (component is IDataFlowComponent<T>)
                PreInitialize(listener, (IDataFlowComponent<T>) component, initializationObjects);
            else
                throw new NotSupportedException(
                    "It looks like you attempted to pre initialize using PreInitializeGeneric but your object was type '" +
                    GetFullName(component.GetType()) + "' and we expected either a source or a component <T> where <T> is:" +
                    GetFullName(typeof (T)));
        }

        public string GetHumanReadableDescription()
        {
            StringBuilder toReturn = new StringBuilder();

            toReturn.Append("Flow is of type " + GetFullName(typeof(T)));

            if (MustHaveSource != null)
                toReturn.Append(", from Source " + GetFullName(MustHaveSource));

            if (MustHaveDestination != null)
                toReturn.Append(", to Destination " + GetFullName(MustHaveDestination));

            if (CannotHave.Any())
                toReturn.Append(" (Forbidden Types Are:" + string.Join(",", CannotHave.Select(GetFullName)) + ")");

            return toReturn.ToString();
        }

        static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;
            StringBuilder sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`")));
            sb.Append(t.GetGenericArguments().Aggregate("<",

                delegate(string aggregate, Type type)
                {
                    return aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type);
                }
                ));
            sb.Append(">");

            return sb.ToString();
        }

        public IEnumerable<Type> GetIPipelineRequirementsForType(Type t)
        {
            return t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineRequirement<>)).Select(r=>r.GetGenericArguments()[0]);
        }

        public Type GetFlowType()
        {
            return typeof (T);
        }
    }
}