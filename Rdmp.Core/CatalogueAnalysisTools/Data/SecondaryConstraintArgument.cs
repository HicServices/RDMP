using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{
    public class SecondaryConstraintArgument: DatabaseEntity
    {

        private string _key;
        private string _value;
        private DQERepository _repository;
        private SecondaryConstraint _constraint;

        public string Key { get => _key; set => SetField(ref _key, value); }
        public string Value { get => _value; set => SetField(ref _value, value); }
        public SecondaryConstraint Constraint { get => _constraint; private set => SetField(ref _constraint, value); }

        public SecondaryConstraintArgument(DQERepository repository, DbDataReader r): base(repository, r)
        {
            _repository = repository;
            _key = r["Key"].ToString();
            _value = r["Value"].ToString();
            _constraint = _repository.GetObjectByID<SecondaryConstraint>(int.Parse(r["SecondaryConstraint_ID"].ToString()));
        }

        public SecondaryConstraintArgument(DQERepository repository, string key, string value, SecondaryConstraint constraint)
        {
            _repository = repository;
            _key = key;
            _value = value;
            _constraint = constraint;

            _repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Key",key },
                {"Value",value },
                {"SecondaryConstraint_ID",constraint.ID }
            });
        }
    }
}
