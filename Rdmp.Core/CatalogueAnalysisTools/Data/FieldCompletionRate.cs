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
using Terminal.Gui.Trees;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{
    public class FieldCompletionRate : DatabaseEntity
    {

        private int _catalogueValidationResultCountsID;

        private int _columnInfoID;

        private float _completionRate;

        private DQERepository _dqeRepository;

        [NoMappingToDatabase]
        public CatalogueValidationResultCounts CatalogueValidationResult { get => _dqeRepository.GetObjectByID<CatalogueValidationResultCounts>(_catalogueValidationResultCountsID); private set => SetField(ref _catalogueValidationResultCountsID, value.ID); }

        [NoMappingToDatabase]
        public ColumnInfo ColumnInfo { get => _dqeRepository.CatalogueRepository.GetObjectByID<ColumnInfo>(_columnInfoID); private set => SetField(ref _columnInfoID, value.ID); }

        public float CompletionRate { get => _completionRate; private set => SetField(ref _completionRate, value); }

        public FieldCompletionRate(DQERepository repository, DbDataReader r) : base(repository, r)
        {
            _dqeRepository = repository;
            _completionRate = float.Parse(r["CompletionRate"].ToString());
            _catalogueValidationResultCountsID = int.Parse(r["CatalogueValidationResultCounts_ID"].ToString());
            _columnInfoID = int.Parse(r["ColumnInfo_ID"].ToString());
        }

        public FieldCompletionRate(DQERepository repository, CatalogueValidationResultCounts resultCounts, ColumnInfo columnInfo, float completionRate)
        {
            _dqeRepository = repository;
            _catalogueValidationResultCountsID = resultCounts.ID;
            _columnInfoID = columnInfo.ID;
            _completionRate = completionRate;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"CompletionRate",_completionRate },
                { "CatalogueValidationResultCounts_ID",_catalogueValidationResultCountsID},
                {"ColumnInfo_ID",_columnInfoID }
            });
        }
    }
}
