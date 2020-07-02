// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline.Requirements;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataFlowPipeline
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

        /// <summary>
        /// The last component in the pipeline, responsible for writing the chunks (of type {T}) to somewhere (E.g. to disk, to database etc)
        /// </summary>
        public IDataFlowDestination<T> Destination { get; private set; }

        /// <summary>
        /// The first component in the pipeline, responsible for iteratively generating chunks (of type {T}) for feeding to downstream pipeline components
        /// </summary>
        public IDataFlowSource<T> Source { get; private set; }

        /// <summary>
        /// Middle components of the pipeline, must be <see cref="IDataFlowComponent{T}"/> with T appropriate to the context.
        /// </summary>
        public List<object> ComponentObjects { get; set; }

        /// <inheritdoc cref="Destination"/>
        public object DestinationObject { get { return Destination; } }

        /// <inheritdoc cref="Source"/>
        public object SourceObject { get { return Source; } }

        /// <summary>
        /// Creates a new pipeline engine ready to run under the <paramref name="context"/> recording events that occur to <paramref name="listener"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="listener"></param>
        /// <param name="pipelineSource"></param>
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

        /// <inheritdoc/>
        public void Initialize(params object[] initializationObjects)
        {
            _context.PreInitialize(_listener,Source,initializationObjects);

            foreach (IDataFlowComponent<T> component in Components)
                _context.PreInitialize(_listener, component, initializationObjects);

            _context.PreInitialize(_listener,Destination,initializationObjects);
            
            initialized = true;
        }

        /// <inheritdoc/>
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
                _listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, e.Message,e));
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
                throw new PipelineCrashedException("Data Flow Pipeline Crashed",exception);
        }

        /// <inheritdoc/>
        public bool ExecuteSinglePass(GracefulCancellationToken cancellationToken)
        {
            if (!initialized)
                throw new Exception("Engine has not been initialized, call Initialize(DataFlowPipelineContext context, params object[] initializationObjects");

            T currentChunk;
            try
            {
                currentChunk = Source.GetChunk(_listener, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
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
                ((DataTable)(object)currentChunk).Clear();
            

            //if the chunk is something that can be disposed, dispose it (e.g. DataTable - to free up memory)
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                ((IDisposable)currentChunk).Dispose();

            return true;
        }

        /// <summary>
        /// Runs checks on all components in the pipeline that support <see cref="ICheckable"/>
        /// </summary>
        /// <param name="notifier"></param>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return _name;
        }
    }
}
