using Rdmp.Core.Curation.Data;
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
    public class CatalogueValidationResult: DatabaseEntity
    {

        private int _catalogueValidationID;
        private DateTime _date;
        private int _correct;
        private int _wrong;
        private int _missing;
        private int _invalid;
        private string _pivotCategory;
        private DQERepository _dqeRepository;

        public CatalogueValidation CatalogueValidation { get => _dqeRepository.GetObjectByID<CatalogueValidation>(_catalogueValidationID); private set => SetField(ref _catalogueValidationID, value.ID); }
        public DateTime Date { get => _date; private set => SetField(ref _date, value); }
        public int Correct { get => _correct; private set => SetField(ref _correct, value); }
        public int Wrong { get => _wrong; private set => SetField(ref _wrong, value); }
        public int Missing { get => _missing; private set => SetField(ref _missing, value); }
        public int Invalid { get => _invalid; private set => SetField(ref _invalid, value); }
        public string PivotCategory { get => _pivotCategory; private set => SetField(ref _pivotCategory, value); }

        public CatalogueValidationResult(DQERepository repository, DbDataReader r) : base(repository, r)
        {
            _dqeRepository = repository;
            DateTime.TryParse(r["Date"].ToString(), out _date);
            _correct = int.Parse(r["Correct"].ToString());
            _wrong = int.Parse(r["Wrong"].ToString());
            _missing = int.Parse(r["Missing"].ToString());
            _invalid = int.Parse(r["Invalid"].ToString());
            _pivotCategory = r["PivotCategory"].ToString();
            _catalogueValidationID  =int.Parse(r["CatalogueValidation_ID"].ToString());
        }

        public CatalogueValidationResult(DQERepository repository, CatalogueValidation validation, DateTime date, string pivotCategory,  int correct, int wrong ,int missing,int invalid)
        {
            _dqeRepository = repository;
            _catalogueValidationID = validation.ID;
            _date = date;
            _pivotCategory = pivotCategory;
            _correct = correct;
            _wrong = wrong; 
            _missing = missing;
            _invalid = invalid;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                { "CatalogueValidation_ID", validation.ID },
                { "PivotCategory", pivotCategory},
                { "Correct", correct},
                {"Wrong",wrong},
                {"Missing",missing },
                {"Invalid",invalid},
                {"Date",date }
            });
        }
    }
}
