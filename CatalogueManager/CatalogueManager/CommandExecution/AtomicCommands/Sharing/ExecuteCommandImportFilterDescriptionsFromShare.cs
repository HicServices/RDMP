using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.CommandExecution.AtomicCommands.Sharing
{
    internal class ExecuteCommandImportFilterDescriptionsFromShare : ExecuteCommandImportShare
    {
        private readonly IFilter _toPopulate;

        public ExecuteCommandImportFilterDescriptionsFromShare(IActivateItems activator, IFilter toPopulate,FileCollectionCommand cmd=null) : base(activator,cmd)
        {
            _toPopulate = toPopulate;

            if(!string.IsNullOrWhiteSpace(_toPopulate.WhereSQL) || !string.IsNullOrWhiteSpace(_toPopulate.Description) || _toPopulate.GetAllParameters().Any())
                SetImpossible("Filter is not empty (import requires a new blank filter)");
        }

        protected override void ExecuteImpl(ShareManager shareManager, List<ShareDefinition> shareDefinitions)
        {
            var definitionToImport = shareDefinitions.First();
            if (!typeof (IFilter).IsAssignableFrom(definitionToImport.Type)) 
                throw new Exception("ShareDefinition was not for an IFilter");

            var props = definitionToImport.Properties;

            //We could be crossing Type boundaries here e.g. importing an ExtractionFilter to overwrite an AggregateFilter so we don't want to use ImportPropertiesOnly
            _toPopulate.Name = props["Name"].ToString();
            _toPopulate.Description = (string) props["Description"];
            _toPopulate.WhereSQL = (string) props["WhereSQL"];
            _toPopulate.IsMandatory = (bool) props["IsMandatory"];
            
            var factory = _toPopulate.GetFilterFactory();

            foreach (var param in shareDefinitions.Skip(1))
            {
                if(!typeof(ISqlParameter).IsAssignableFrom(param.Type))
                    throw new Exception("Expected ShareDefinition to start with 1 IFilter then have 0+ ISqlParameters instead we found a " + param.Type);

                var paramProps = param.Properties;

                var newParam = factory.CreateNewParameter(_toPopulate, (string) paramProps["ParameterSQL"]);
                newParam.Comment = (string)paramProps["Comment"];
                newParam.Value = (string)paramProps["Value"];
                
                newParam.SaveToDatabase();
            }

            _toPopulate.SaveToDatabase();
            Publish((DatabaseEntity)_toPopulate);
        }
    }
}