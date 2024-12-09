using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data;

public class CatalogueOverviewDataPoint : DatabaseEntity, ICatalogueOverviewDataPoint
{

    private int _catalogueOverview_ID;
    private DateTime _date;
    private int _count;
    public int CatalogueOverview_ID { get => _catalogueOverview_ID; protected set => SetField(ref _catalogueOverview_ID, value); }

    public DateTime Date { get => _date; set => SetField(ref _date, value); }
    public int Count { get => _count; set => SetField(ref _count, value); }

    public CatalogueOverviewDataPoint() { }

    public CatalogueOverviewDataPoint(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        _catalogueOverview_ID = int.Parse(r["CatalogueOverview_ID"].ToString());
        Date = DateTime.Parse(r["Date"].ToString());
        Count = int.Parse(r["Count"].ToString());
    }

    public CatalogueOverviewDataPoint(ICatalogueRepository repository, int catalogueOverview_ID, DateTime date, int count)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"CatalogueOverview_ID", catalogueOverview_ID},
                { "Date", date},
                {"Count", count}
            });
    }
}
