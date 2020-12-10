using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.DataViewing
{
    class ViewSelectedDatasetExtractionUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection
    {
        private ExtractDatasetCommand _request;

        ISelectedDataSets SelectedDataset{get => DatabaseObjects.OfType<ISelectedDataSets>().FirstOrDefault();}

        public ViewSelectedDatasetExtractionUICollection()
        {
        }

        public ViewSelectedDatasetExtractionUICollection(ISelectedDataSets dataset) : this()
        {
            DatabaseObjects.Add(dataset);
        }

        public string GetSql()
        {
            BuildRequest();
            
            //get the SQL from the query builder 
            return _request.QueryBuilder.SQL;
        }

        private void BuildRequest()
        {
            if(_request != null)
                return;

            var ec = SelectedDataset.ExtractionConfiguration;

            if(ec.Cohort_ID == null)
                throw new Exception("No cohort has been defined for this ExtractionConfiguration");

            //We are generating what the extraction SQL will be like, that only requires the dataset so empty bundle is fine
            _request = new ExtractDatasetCommand(ec,new ExtractableDatasetBundle(SelectedDataset.ExtractableDataSet));
            _request.GenerateQueryBuilder();
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            BuildRequest();

            return _request?.QueryBuilder?.TablesUsedInQuery?.FirstOrDefault();
        }

        public IEnumerable<DatabaseEntity> GetToolStripObjects()
        {
            yield return (DatabaseEntity)SelectedDataset;
        }

        public string GetTabName()
        {
            return "Extract " + SelectedDataset;
        }

        public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
        {
            
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            BuildRequest();

            return _request.QueryBuilder.QuerySyntaxHelper;
        }
    }
}
