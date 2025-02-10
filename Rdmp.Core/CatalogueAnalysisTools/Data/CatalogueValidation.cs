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
    public  class CatalogueValidation: DatabaseEntity
    {

        private Catalogue _catalogue;
        private DateTime _date;


        public Catalogue Catalogue { get => _catalogue; private set => SetField(ref _catalogue, value); }
        public DateTime Date{ get => _date; private set => SetField(ref _date, value); }

        public CatalogueValidation(DQERepository repository, DbDataReader r) : base(repository, r)
        {
            _catalogue = repository.CatalogueRepository.GetObjectByID<Catalogue>(int.Parse(r["Catalogue_ID"].ToString()));
            DateTime.TryParse(r["Date"].ToString(), out _date);
        }
        public CatalogueValidation(DQERepository repository, Catalogue catalogue)
        {
            repository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "Catalogue_ID", catalogue.ID },
                { "Date", DateTime.Now}
            });
        }
    }
}
