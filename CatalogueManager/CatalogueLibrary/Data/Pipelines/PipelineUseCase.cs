using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Abstract base IPipelineUseCase. Provides basic implementations for filtering compatible pipelines and translating
    /// a selected IPipeline into an actual executable engine instance via DataFlowPipelineEngineFactory.  Set ExplicitSource / 
    /// ExplicitDestination / PreInitialize objects etc as needed for your use case.
    /// </summary>
    public abstract class PipelineUseCase : IPipelineUseCase
    {
        /// <summary>
        /// Array of all the objects available for executing the Pipeline.  
        /// <para>OR: If <see cref="IsDesignTime"/> then an array of the Types of objects that should be around at runtime
        /// when performing the task described by the PipelineUseCase</para>
        /// </summary>
        /// <returns></returns>
        public HashSet<object> GetInitializationObjects()
        {
            return InitializationObjects;
        }

        public IDataFlowPipelineContext GetContext()
        {
            return _context;
        }

        protected abstract IDataFlowPipelineContext GenerateContext();

        public object ExplicitSource { get; protected set; }
        public object ExplicitDestination { get; protected set; }

        /// <summary>
        /// True if there there are no objects available for hydrating (e.g. no files to load, no picked cohorts etc).  This is often 
        /// the case when the user is editing a <see cref="Pipeline"/> at some arbitrary time.
        /// 
        /// <para>If this is true then GetInitializationObjects should return Type[] instead of the actually selected objects for the task</para>
        /// </summary>
        public bool IsDesignTime { get; private set; }
        
        protected HashSet<object> InitializationObjects = new HashSet<object>();
        private IDataFlowPipelineContext _context;

        /// <summary>
        /// The normal (non desing time) constructor.  Add your objects to <see cref="InitializationObjects"/>
        /// </summary>
        protected PipelineUseCase()
        {
            IsDesignTime = false;
            _context = GenerateContext();
        }

        /// <summary>
        /// Use this constructor if you are intending to use the <see cref="PipelineUseCase"/> for design time activities only (building pipelines
        /// from compatible components).  Only use if you don't have all the normally required object instances to actually execute a pipeline.
        /// </summary>
        /// <param name="designTimeInitializationObjectTypes"></param>
        protected PipelineUseCase(Type[] designTimeInitializationObjectTypes)
        {
            IsDesignTime = true;
            foreach (Type t in designTimeInitializationObjectTypes)
                InitializationObjects.Add(t);

            _context = GenerateContext();
        }

        public virtual IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines)
        {
            var context = GetContext();
            return pipelines.Where(context.IsAllowable);
        }
        
        public virtual IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener)
        {
            var engine = new DataFlowPipelineEngineFactory(this, pipeline).Create(pipeline, listener);
            engine.Initialize(GetInitializationObjects().ToArray());

            return engine;
        }

        protected void AddInitializationObject(object o)
        {
            if (o != null)
                InitializationObjects.Add(o);
        }
    }
}