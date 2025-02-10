using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools
{
    public class MinMaxFinder
    {
        private ICatalogueRepository _catalogueRepository;
        private ColumnInfo _column;
        private ColumnInfo _bucketColumn;
        private int _minBucketSize;
        public MinMaxFinder(ICatalogueRepository catalogueRepository,ColumnInfo column, ColumnInfo bucketColumn,int minBukcetSize)
        {
            //this assumes that each value in the bucket column is a new bucket
            _catalogueRepository = catalogueRepository;
            _column = column;
            _bucketColumn = bucketColumn;
            _minBucketSize = minBukcetSize;
        }
    }
}
