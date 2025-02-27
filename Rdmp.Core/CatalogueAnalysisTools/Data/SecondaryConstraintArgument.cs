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
        private int _constraintID;

        public string Key { get => _key; set => SetField(ref _key, value); }
        public string Value { get => _value; set => SetField(ref _value, value); }
        public int ConstraintID { get => _constraintID; private set => SetField(ref _constraintID, value); }

        public SecondaryConstraintArgument(DQERepository repository, DbDataReader r): base(repository, r)
        {
            _repository = repository;
            _key = r["Key"].ToString();
            _value = r["Value"].ToString();
            _constraintID =int.Parse(r["SecondaryConstraint_ID"].ToString());
        }

        public SecondaryConstraintArgument(DQERepository repository, string key, string value, SecondaryConstraint constraint)
        {
            _repository = repository;
            _key = key;
            _value = value;
            _constraintID = constraint.ID;

            _repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Key",key },
                {"Value",value },
                {"SecondaryConstraint_ID",constraint.ID }
            });
        }
    }
}
