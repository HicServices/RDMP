using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.Repositories;
using System.Data.Common;
using Rdmp.Core.Curation.Data.Defaults;
using Microsoft.IdentityModel.Tokens;

namespace Rdmp.Core.Curation.Data;

public class CatalogueOverview : DatabaseEntity, ICatalogueOverview
{

    private int _catalogue_ID;
    private DateTime? _lastDataLoad;
    private DateTime? _lastExtractionTime;
    private int _numberOfRecords;
    private int _numberOfPeople;
    private DateTime? _startDate;
    private DateTime? _endDate;


    [Unique]
    [NotNull]
    [DoNotImportDescriptions]
    public int Catalogue_ID
    {
        get => _catalogue_ID;
        protected set => SetField(ref _catalogue_ID, value);
    }

    [DoNotImportDescriptions]
    public DateTime? LastDataLoad
    {
        get => _lastDataLoad;
        set => SetField(ref _lastDataLoad, value);
    }

    [DoNotImportDescriptions]
    public DateTime? LastExtractionTime
    {
        get => _lastExtractionTime;
        set => SetField(ref _lastExtractionTime, value);
    }

    [DoNotImportDescriptions]
    public int NumberOfRecords
    {
        get => _numberOfRecords;
        set => SetField(ref _numberOfRecords, value);
    }

    [DoNotImportDescriptions]
    public int NumberOfPeople
    {
        get => _numberOfPeople;
        set => SetField(ref _numberOfPeople, value);
    }

    [DoNotImportDescriptions]
    public DateTime? StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    [DoNotImportDescriptions]
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    public CatalogueOverview() { }

    internal CatalogueOverview(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString());
        LastDataLoad = !string.IsNullOrEmpty(r["LastDataLoad"].ToString()) ? DateTime.Parse(r["LastDataLoad"].ToString()) : null;
        LastExtractionTime = !string.IsNullOrEmpty(r["LastExtractionTime"].ToString())? DateTime.Parse(r["LastExtractionTime"].ToString()) : null;
        NumberOfRecords = int.Parse(r["NumberOfRecords"].ToString());
        NumberOfPeople = int.Parse(r["NumberOfPeople"].ToString());
        StartDate = !string.IsNullOrEmpty(r["StartDate"].ToString())? DateTime.Parse(r["StartDate"].ToString()) : null;
        EndDate = !string.IsNullOrEmpty(r["EndDate"].ToString())? DateTime.Parse(r["EndDate"].ToString()) : null;
    }

    public CatalogueOverview(ICatalogueRepository repository, int catalogueID, int numberOfRecords, int numberOfPeople)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Catalogue_ID", catalogueID },
                { "NumberOfRecords", numberOfRecords},
                {"NumberOfPeople", numberOfPeople }
            });
    }
}