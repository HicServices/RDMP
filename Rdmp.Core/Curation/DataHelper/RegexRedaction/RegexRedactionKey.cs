using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public class RegexRedactionKey : DatabaseEntity, IRegexRedactionKey
    {
        private int _redaction;
        private int _columnInfo;
        private string _value;

        public int RegexRedaction_ID { get => _redaction; set => SetField(ref _redaction, value); }
        public int ColumnInfo_ID { get => _columnInfo; set => SetField(ref _columnInfo, value); }

        public string Value { get => _value; set => SetField(ref _value, value); }

        public RegexRedactionKey() { }

        public RegexRedactionKey(ICatalogueRepository repository, RegexRedaction redaction, ColumnInfo pkColumn, string value)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"RegexRedaction_ID", redaction.ID },
                {"ColumnInfo_ID", pkColumn.ID },
                {"Value", value },
            });
        }

        internal RegexRedactionKey(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            _redaction = Int32.Parse(r["RegexRedaction_ID"].ToString());
            _columnInfo = Int32.Parse(r["ColumnInfo_ID"].ToString());
            _value = r["Value"].ToString();
        }

    }
}
