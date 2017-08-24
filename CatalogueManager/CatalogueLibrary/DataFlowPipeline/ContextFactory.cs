using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.DataFlowPipeline
{
    public class ContextFactory
    {
        private readonly MEF _mefPlugins;

        public ContextFactory(MEF mefPlugins)
        {
            _mefPlugins = mefPlugins;
        }

        public object CreateObject(string contextFieldPath)
        {
            var contextField = GetContextField(contextFieldPath);
            if (contextField == null)
                throw new Exception("There is no field called " + contextFieldPath);

            return contextField.GetValue(null);
        }

        public FieldInfo GetContextField(string contextFieldPath)
        {
            // first get the parent typename from the pipelineContextType string so we can look it up
            var parentTypeName = contextFieldPath.Substring(0, contextFieldPath.LastIndexOf(".", StringComparison.Ordinal));

            // get the pipeline context type so we can then extract the Context field from it, which contains the pipeline's data type
            var parentType = _mefPlugins.GetTypeByNameFromAnyLoadedAssembly(parentTypeName);
            if (parentType == null)
                throw new Exception("Could not find type '" + parentTypeName + "' for " + contextFieldPath);

            // now get the static context field
            var contextFieldName = contextFieldPath.Substring(contextFieldPath.LastIndexOf(".", StringComparison.Ordinal) + 1);
            return parentType.GetFields().SingleOrDefault(info => info.Name == contextFieldName);
        }
    }
}