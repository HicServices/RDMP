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
    class UserDefinedChartResult: DatabaseEntity
    {
        private string _x;
        private string _y;
        private UserDefinedChart _userDefinedChart;
        private DQERepository _DQERepository { get; set; }


        public string X { get => _x; set => SetField(ref _x, value); }
        public string Y { get => _x; set => SetField(ref _x, value); }

        public UserDefinedChart UserDefinedChart { get => _userDefinedChart; set => SetField(ref _userDefinedChart, value); }


        public UserDefinedChartResult(DQERepository repository, DbDataReader r): base(repository, r)
        {
            _DQERepository = repository;
            _x = r["X"].ToString();
            _y = r["Y"].ToString();
            _userDefinedChart = _DQERepository.GetObjectByID<UserDefinedChart>(int.Parse(r["UserDefinedChart_ID"].ToString()));
        }

        public UserDefinedChartResult(DQERepository repository,UserDefinedChart userDefinedChart, string x, string y)
        {
            _DQERepository = repository;
            _userDefinedChart = userDefinedChart;
            _x = x;
            _y = y;

            _DQERepository.InsertAndHydrate(this, new Dictionary<string, object> {
                {"UserDefinedChart_ID", userDefinedChart.ID },
                {"X",x },
                {"Y",y }
            });
        }
    }
}
