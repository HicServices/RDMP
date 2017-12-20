using System;
using System.Collections.Generic;
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

        public List<IDataFlowComponent<T>> Components { get; set; }
        public IDataFlowDestination<T> Destination { get; private set; }
        public IDataFlowSource<T> Source { get; private set; }

        public List<object> ComponentObjects
        {
            get { return Components.Cast<object>().ToList(); }
        }
        public object DestinationObject { get { return Destination; } }
        public object SourceObject { get { return Source; } }

        public DataFlowPipelineEngine(DataFlowPipelineContext<T> context,IDataFlowSource<T> source, IDataFlowDestination<T> destination, IDataLoadEventListener listener)
        {
            Source = source;
            Destination = destination;
            _context = context;
            _listener = listener;
            Components = new List<IDataFlowComponent<T>>();
        }

        public void Initialize(params object[] initializationObjects)
        {
            _context.PreInitialize(_listener,Source,initializationObjects);

            foreach (IDataFlowComponent<T> component in Components)
                _context.PreInitialize(_listener, component, initializationObjects);

            _context.PreInitialize(_listener,Destination,initializationObjects);
            
            initialized = true;
        }


        private bool initialized = false;
        public void ExecutePipeline(GracefulCancellationToken cancellationToken)
        {
            Exception exception = null;
            try
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Starting pipeline engine"));

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
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing to Dispose of DataFlowPipelineEngine components"));

                foreach (IDataFlowComponent<T> dataLoadComponent in Components)
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to Dispose " + dataLoadComponent));
                    try
                    {

                        dataLoadComponent.Dispose(_listener, exception);
                    }
                    catch (Exception e)
                    {
                        _listener.OnNotify(dataLoadComponent,new NotifyEventArgs(ProgressEventType.Error, "Error Disposing Component",e));
                    }
                }

                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to Dispose " + Source));
                try
                {
                    Source.Dispose(_listener, exception);
                }
                catch (Exception e)
                {
                    _listener.OnNotify(Source, new NotifyEventArgs(ProgressEventType.Error, "Error Disposing Source Component", e));
                }

                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to Dispose " + Destination));
                try
                {

                    Destination.Dispose(_listener, exception);
                }
                catch (Exception e)
                {
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
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Received null chunk from the Source component, stopping engine"));
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
    }
}
