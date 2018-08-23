using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Generic implementation of IDataFlowPipelineEngine (See IDataFlowPipelineEngine).  You can create a DataFlowPipelineEngine by manually constructing the context,
    /// source, destination etc but more often you will want to use an IPipeline configured by the user and an IPipelineUseCase to stamp out the pipeline into an instance
    /// of the engine (See IDataFlowPipelineEngineFactory).  IPipeline is the user configured set of components they think will achieve a given task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataFlowPipelineEngine<T> : IDataFlowPipelineEngine
    {
        private readonly DataFlowPipelineContext<T> _context;
        private readonly IDataLoadEventListener _listener;

        private bool initialized = false;
        private string _name;

        /// <summary>
        /// Readonly cast of <see cref="ComponentObjects"/>. If you need to add components, add them to <see cref="ComponentObjects"/> instead.
        /// </summary>
        public ReadOnlyCollection<IDataFlowComponent<T>> Components { get { return ComponentObjects.Cast<IDataFlowComponent<T>>().ToList().AsReadOnly(); } }

        public IDataFlowDestination<T> Destination { get; private set; }
        public IDataFlowSource<T> Source { get; private set; }

        /// <summary>
        /// Middle components of the pipeline, must be <see cref="IDataFlowComponent"/> with T appropriate to the context.
        /// </summary>
        public List<object> ComponentObjects { get; set; }

        public object DestinationObject { get { return Destination; } }
        public object SourceObject { get { return Source; } }

        public DataFlowPipelineEngine(DataFlowPipelineContext<T> context,IDataFlowSource<T> source, 
                                      IDataFlowDestination<T> destination, IDataLoadEventListener listener,
                                      IPipeline pipelineSource = null)
        {
            Source = source;
            Destination = destination;
            _context = context;
            _listener = listener;
            ComponentObjects = new List<object>();

            if (pipelineSource != null)
                _name = pipelineSource.Name;
            else
                _name = "Undefined pipeline";
        }

        public void Initialize(params object[] initializationObjects)
        {
            _context.PreInitialize(_listener,Source,initializationObjects);

            foreach (IDataFlowComponent<T> component in Components)
                _context.PreInitialize(_listener, component, initializationObjects);

            _context.PreInitialize(_listener,Destination,initializationObjects);
            
            initialized = true;
        }

        public void ExecutePipeline(GracefulCancellationToken cancellationToken)
        {
            Exception exception = null;
            try
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, "Starting pipeline engine"));

                if (!initialized)
                    throw new Exception(
                        "Engine has not been initialized, call Initialize(DataFlowPipelineContext context, params object[] initializationObjects");

                var hasMoreData = true;
                while (!cancellationToken.IsCancellationRequested && hasMoreData)
                    hasMoreData = ExecuteSinglePass(cancellationToken);

                if (cancellationToken.IsAbortRequested)
                {
                    Source.Abort(_listener);

                    foreach (var c in Components)
                        c.Abort(_listener);

                    Destination.Abort(_listener);

                    _listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information, "Pipeline engine aborted"));
                    return;
                }
            }
            catch (Exception e)
            {
                exception = e;
                _listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Data Flow Pipeline Engine execution threw Exception",e));
            }
            finally
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, "Preparing to Dispose of DataFlowPipelineEngine components"));
                
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace, "About to Dispose " + Source));
                try
                {
                    Source.Dispose(_listener, exception);
                }
                catch (Exception e)
                {
                    //dispose crashing is only a dealbreaker if there wasn't already an exception in the pipeline
                    if (exception == null)
                        throw;

                    _listener.OnNotify(Source, new NotifyEventArgs(ProgressEventType.Error, "Error Disposing Source Component", e));
                }

                foreach (IDataFlowComponent<T> dataLoadComponent in Components)
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace, "About to Dispose " + dataLoadComponent));
                    try
                    {
                        dataLoadComponent.Dispose(_listener, exception);
                    }
                    catch (Exception e)
                    {
                        //dispose crashing is only a dealbreaker if there wasn't already an exception in the pipeline
                        if (exception == null)
                            throw;

                        _listener.OnNotify(dataLoadComponent,new NotifyEventArgs(ProgressEventType.Error, "Error Disposing Component",e));
                    }
                }

                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace, "About to Dispose " + Destination));
                try
                {
                    Destination.Dispose(_listener, exception);
                }
                catch (Exception e)
                {
                    //dispose crashing is only a dealbreaker if there wasn't already an exception in the pipeline
                    if (exception == null)
                        throw;

                    _listener.OnNotify(Destination, new NotifyEventArgs(ProgressEventType.Error, "Error Disposing Destination Component", e));
                }
            }

            if (exception != null)
                throw new Exception("Data Flow Pipeline Crashed",exception);
        }

        public bool ExecuteSinglePass(GracefulCancellationToken cancellationToken)
        {
            if (!initialized)
                throw new Exception("Engine has not been initialized, call Initialize(DataFlowPipelineContext context, params object[] initializationObjects");

            T currentChunk;
            try
            {
                currentChunk = Source.GetChunk(_listener, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error when attempting to get a chunk from the source component: " + Source, e);
            }

            if (currentChunk == null)
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, "Received null chunk from the Source component, stopping engine"));
                return false;
            }

            foreach (IDataFlowComponent<T> component in Components)
            {
                if (cancellationToken.IsAbortRequested) break;
                currentChunk = component.ProcessPipelineData( currentChunk, _listener, cancellationToken);
            }

            if (cancellationToken.IsAbortRequested) return true;
            Destination.ProcessPipelineData(currentChunk, _listener, cancellationToken);

            if (cancellationToken.IsAbortRequested) return true;

            //if it is a DataTable call .Clear() because Dispose doesn't actually free up any memory
            if (typeof(DataTable).IsAssignableFrom(typeof(T)))
            {
                ((DataTable)(object)currentChunk).Clear();
                GC.Collect();
            }

            //if the chunk is something that can be disposed, dispose it (e.g. DataTable - to free up memory)
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                ((IDisposable)currentChunk).Dispose();

            return true;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                ICheckable checkableSource = Source as ICheckable;
                if (checkableSource != null)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("About to start checking Source component " + checkableSource,
                        CheckResult.Success));
                    checkableSource.Check(notifier);
                }
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Source component " + Source + " does not support ICheckable so skipping checking it",
                            CheckResult.Warning));

                foreach (IDataFlowComponent<T> component in Components)
                {
                    ICheckable checkable = component as ICheckable;

                    if (checkable != null)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("About to start checking component " + component, CheckResult.Success));
                        checkable.Check(notifier);
                    }
                    else
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "Component " + component + " does not support ICheckable so skipping checking it",
                                CheckResult.Warning));
                }

                ICheckable checkableDestination = Destination as ICheckable;
                if (checkableDestination != null)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("About to start checking Destination component " + checkableDestination,
                        CheckResult.Success));
                    checkableDestination.Check(notifier);
                }
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Destination component " + Destination + " does not support ICheckable so skipping checking it",
                            CheckResult.Warning));

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        typeof (DataFlowPipelineEngine<T>).Name + " Checking failed in an unexpected way",
                        CheckResult.Fail, e));
            }


            notifier.OnCheckPerformed(new CheckEventArgs("Finished checking all components",CheckResult.Success));
                 
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
