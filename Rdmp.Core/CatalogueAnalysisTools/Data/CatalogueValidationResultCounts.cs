using Amazon.Runtime.Internal.Transform;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{
    public class CatalogueValidationResultCounts : DatabaseEntity
    {

        private int _catalogueValidationResultID;

        private int _recordCount;
        private int _extractionIdentifierCount;

        private List<FieldCompletionRate> _fieldCompletionRates;

        private DQERepository _dqeRepository;


        [NoMappingToDatabase]
        public CatalogueValidationResult CatalogueValidationResult { get => _dqeRepository.GetObjectByID<CatalogueValidationResult>(_catalogueValidationResultID); private set => SetField(ref _catalogueValidationResultID, value.ID); }

        public int RecordCount { get => _recordCount; private set => SetField(ref _recordCount, value); }
        public int ExtractionIdentifierCount { get => _extractionIdentifierCount; private set => SetField(ref _extractionIdentifierCount, value); }

        [NoMappingToDatabase]
        public List<FieldCompletionRate> CompletionRates {
            get => GetCompletionRates();
            private set => SetField(ref _fieldCompletionRates, value); }


        private List<FieldCompletionRate> GetCompletionRates()
        {
            {
                if (_fieldCompletionRates == null)
                {
                    _fieldCompletionRates = _dqeRepository.GetAllObjects<FieldCompletionRate>().Where(fcr => fcr.CatalogueValidationResult.ID == this.ID ).ToList();
                }
                return _fieldCompletionRates;
            }
        }

        public CatalogueValidationResultCounts(DQERepository repository, DbDataReader r) : base(repository, r)
        {
            _dqeRepository = repository;
            _recordCount = int.Parse(r["RecordCount"].ToString());
            _extractionIdentifierCount = int.Parse(r["ExtractionIdentifierCount"].ToString());
            _catalogueValidationResultID = int.Parse(r["CatalogueValidationResult_ID"].ToString());

        }

        public CatalogueValidationResultCounts(DQERepository repository, CatalogueValidationResult validationResult, int recordCount, int extractionIdentifierCount)
        {

            _dqeRepository = repository;
            _recordCount = recordCount;
            _extractionIdentifierCount = extractionIdentifierCount;
            _catalogueValidationResultID = validationResult.ID;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"RecordCount",_recordCount },
                {"ExtractionIdentifierCount",_extractionIdentifierCount },
                {"CatalogueValidationResult_ID",_catalogueValidationResultID }
            });


        }
    }
}
