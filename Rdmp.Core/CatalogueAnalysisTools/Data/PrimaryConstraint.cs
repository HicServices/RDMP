using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{

    public class PrimaryConstraint: DatabaseEntity
    {

        public enum Constraints 
        {
            ALPHA,
            ALPHANUMERIC,
            CHI,
            DATE
        }

        public enum ConstraintResults
        {
            CORRECT,
            WRONG,
            MISSING,
            INVALID
        }


        private DQERepository _DQERepository { get; set; }
        private ColumnInfo _columnInfo;
        private Constraints _constraint;
        private ConstraintResults _result;


        public ColumnInfo ColumnInfo { get => _columnInfo; private set => SetField(ref _columnInfo, value); }
        public Constraints Constraint { get => _constraint; set => SetField(ref _constraint, value); }
        public ConstraintResults Result { get => _result; set => SetField(ref _result, value); }

        public PrimaryConstraint(DQERepository repository, DbDataReader r): base(repository,r) {
            _DQERepository = repository;
            _columnInfo = _DQERepository.CatalogueRepository.GetObjectByID<ColumnInfo>(int.Parse(r["ColumnInfo_ID"].ToString()));
            _constraint = (Constraints)int.Parse(r["Constraint"].ToString());
            _result = (ConstraintResults)int.Parse(r["Result"].ToString());
        }

        public PrimaryConstraint(DQERepository repository, ColumnInfo columnInfo, Constraints constraint, ConstraintResults result)
        {
            _DQERepository = repository;
            _columnInfo = columnInfo;
            _constraint = constraint;
            _result = result;

            _DQERepository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "ColumnInfo_ID", columnInfo.ID },
                { "Constraint", (int)constraint},
                { "Result", (int)result}
            });
        }
    }
}
