using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation.Dependency.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{
    public  class CatalogueValidation: DatabaseEntity
    {
        private DQERepository _dqeRepositroy;

        private Catalogue _catalogue;
        private DateTime _date;
        private int _timeColumnID;
        private int _pivotCategoryID;

        public DataTable GenerateDataTable(string category="ALL")
        {
            var dt = new DataTable();
            dt.Columns.Add("YearMonth");
            dt.Columns.Add("Year");
            dt.Columns.Add("Month");
            dt.Columns.Add("Correct");
            dt.Columns.Add("Wrong");
            dt.Columns.Add("Missing");
            dt.Columns.Add("Invalid");
            var results = _dqeRepositroy.GetAllObjectsWhere<CatalogueValidationResult>("CatalogueValidation_ID", this.ID).Where(cvr => cvr.PivotCategory == category).OrderBy(item => item.Date);
            foreach (var result in results) {
                dt.Rows.Add(new object[] { result.Date.ToString("yyyy-MM"), result.Date.Year,result.Date.Month, result.Correct, result.Wrong, result.Missing, result.Invalid }); 
            }

            return dt;
        }
        public Catalogue Catalogue { get => _catalogue; private set => SetField(ref _catalogue, value); }
        public DateTime Date{ get => _date; private set => SetField(ref _date, value); }

        //todo
        public int TimeColumn_ID { get => _timeColumnID; set=> SetField(ref _timeColumnID,value); }

        //todo
        public int PivotColumn_ID { get =>_pivotCategoryID; set => SetField(ref _pivotCategoryID,value); }

        public CatalogueValidation(DQERepository repository, DbDataReader r) : base(repository, r)
        {
            _dqeRepositroy = repository;
            _catalogue = repository.CatalogueRepository.GetObjectByID<Catalogue>(int.Parse(r["Catalogue_ID"].ToString()));
            DateTime.TryParse(r["Date"].ToString(), out _date);
            _timeColumnID = int.Parse(r["TimeColumn_ID"].ToString());
            _pivotCategoryID = int.Parse(r["PivotColumn_ID"].ToString());
        }
        public CatalogueValidation(DQERepository repository, Catalogue catalogue, ColumnInfo timeColumn, ColumnInfo pivotColumn)
        {
            _dqeRepositroy = repository;
            repository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "Catalogue_ID", catalogue.ID },
                { "Date", DateTime.Now},
                {"TimeColumn_ID", timeColumn.ID },
                {"PivotColumn_ID", pivotColumn.ID }
            });
        }
    }
}
