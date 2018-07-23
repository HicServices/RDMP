using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;

namespace CachingEngine.Locking
{
    /// <summary>
    /// Manages an 'engineMap' collection of ILoadProgress and provides functionality to lock/unlock the ILoadProgress throughout the lifetime of the
    /// executing IDataFlowPipelineEngine
    /// </summary>
    public class LoadCacheEngineLockProvider : IEngineLockProvider
    {
        private readonly Dictionary<IDataFlowPipelineEngine, ILoadProgress> _engineMap;

        public LoadCacheEngineLockProvider(Dictionary<IDataFlowPipelineEngine, ILoadProgress> engineMap)
        {
            _engineMap = engineMap;
        }

        public bool IsLocked(IDataFlowPipelineEngine engine)
        {
            CheckExists(engine);
            var loadProgress = _engineMap[engine];
            return loadProgress.IsDisabled;
        }

        public string Details(IDataFlowPipelineEngine engine)
        {
            CheckExists(engine);
            return "Engine for Load Schedule '" + _engineMap[engine].Name + "'";
        }


        private void CheckExists(IDataFlowPipelineEngine engine)
        {
            if (!_engineMap.ContainsKey(engine))
                throw new Exception("Lock provider does not know about engine <details>");
        }
    }
}