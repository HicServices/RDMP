using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using RDMPObjectVisualisation.Pipelines;

namespace CatalogueManager.SimpleDialogs.SimpleFileImporting
{
    public class ConfigurePipelineUIFactory
    {
        private readonly MEF _mef;
        private readonly CatalogueRepository _repository;

        public ConfigurePipelineUIFactory(MEF mef, CatalogueRepository repository)
        {
            _mef = mef;
            _repository = repository;
        }

        // mmmm, type-safety for the win
        public Form Create(string dataFlowType, object pipeline, object source, object destination, object context, List<object> initializationObjectsForPreviewPipelineSource)
        {
            var fixedType = typeof(ConfigurePipelineUI<>);
            var specificType = fixedType.MakeGenericType(_mef.GetTypeByNameFromAnyLoadedAssembly(dataFlowType));
            var form = Activator.CreateInstance(specificType, pipeline, source, destination, context, initializationObjectsForPreviewPipelineSource, _repository);
            var castForm = form as Form;
            if (castForm == null)
                throw new InvalidCastException("Could not cast the dynamically created form. Specific type = " + fixedType + ". Generic type = " + dataFlowType);

            return castForm;
        }
    }
}