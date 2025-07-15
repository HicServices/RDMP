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
using static Rdmp.Core.CatalogueAnalysisTools.Data.PrimaryConstraint;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{
    public class SecondaryConstraint : DatabaseEntity
    {
        public enum Constraints
        {
            BOUNDDOUBLE,
            BOUNDDATE,
            NOTNULL,
            REGULAREXPRESSION
        }

        public enum Consequences
        {
            MISSING,
            WRONG,
            INVALIDATESROW
        }

        private DQERepository _DQERepository { get; set; }
        private ColumnInfo _columnInfo;
        private int _columnInfoID;
        private Constraints _constraint;
        private Consequences _consequence;
        //private SecondaryConstraintArgument[] _arguments;

        public ColumnInfo ColumnInfo { get => _columnInfo; private set => SetField(ref _columnInfo, value); }
        public Consequences Consequence { get => _consequence; set => SetField(ref _consequence, value); }
        public Constraints Constraint { get => _constraint; set => SetField(ref _constraint, value); }

        //public SecondaryConstraintArgument[] Arguments { get => _arguments == null ? GetArguments() : _arguments; set => SetField(ref _arguments, value); }


        public SecondaryConstraintArgument[] GetArguments()
        {
            return  _DQERepository.GetAllObjectsWhere<SecondaryConstraintArgument>("SecondaryConstraint_ID", this.ID);
            //return _arguments;

        }

        public SecondaryConstraint() { }

        public SecondaryConstraint(DQERepository repository, DbDataReader r) : base(repository, r)
        {
            _DQERepository = repository;
            _columnInfo = _DQERepository.CatalogueRepository.GetObjectByID<ColumnInfo>(int.Parse(r["ColumnInfo_ID"].ToString()));
            _columnInfoID = (int.Parse(r["ColumnInfo_ID"].ToString()));
            _constraint = (Constraints)int.Parse(r["Constraint"].ToString());
            _consequence = (Consequences)int.Parse(r["Consequence"].ToString());
        }

        public SecondaryConstraint(DQERepository repository, ColumnInfo columnInfo, Constraints constraint, Consequences consequence)
        {
            _DQERepository = repository;
            _columnInfo = columnInfo;
            _consequence = consequence;
            _constraint = constraint;
            _DQERepository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                 { "ColumnInfo_ID", columnInfo.ID },
                { "Constraint", (int)constraint},
                { "Consequence", (int)consequence}
            });

        }
    }
}
