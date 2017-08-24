using System;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using DataQualityEngine;

namespace CatalogueWebService.Modules.Data
{
    public class CatalogueData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }

        public string DatasetTimeCoverage { get; set; }
        public string Description { get; set; }
        public Uri Detail_Page_URL { get; set; }
        public string Geographical_coverage { get; set; }
        public string Background_summary { get; set; }
        public string Search_keywords { get; set; }

        public CatalogueData()
        {
        }

        public CatalogueData(Catalogue catalogue,IDetermineDatasetTimespan timespanCaluclator)
        {
            ID = catalogue.ID;
            Name = catalogue.Name;
            Acronym = catalogue.Acronym;

            DatasetTimeCoverage = timespanCaluclator.GetHumanReadableTimepsanIfKnownOf(catalogue, true);
            Description = catalogue.Description;
            Detail_Page_URL = catalogue.Detail_Page_URL;
            Geographical_coverage = catalogue.Geographical_coverage;
            Background_summary = catalogue.Background_summary;
            Search_keywords = catalogue.Search_keywords;
        }
    }
}