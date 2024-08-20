using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public class RegexRedaction : DatabaseEntity, IRegexRedaction

    {
        private int _redactionConfigurationID;
        private int _startingIndex;
        private string _redactedValue;
        private string _replacementValue;
        private int _columnInfoID;

        public int RedactionConfiguration_ID
        {
            get => _redactionConfigurationID;
            set => SetField(ref _redactionConfigurationID, value);
        }

        public string RedactedValue
        {
            get => _redactedValue;
            set => SetField(ref _redactedValue, value.ToString());
        }

        public string ReplacementValue
        {
            get => _replacementValue;
            set => SetField(ref _replacementValue, value.ToString());
        }

        public int StartingIndex
        {
            get => _startingIndex;
            set => SetField(ref _startingIndex, value);
        }

        public int ColumnInfo_ID
        {
            get => _columnInfoID;
            set => SetField(ref _columnInfoID, value);
        }

        [NoMappingToDatabase]
        public List<RegexRedactionKey> RedactionKeys => [.. CatalogueRepository.GetAllObjectsWhere<RegexRedactionKey>("RegexRedaction_ID", this.ID)];


        public RegexRedaction() { }

        public RegexRedaction(ICatalogueRepository repository, int redactionConfigurationID, int startingIndex, string redactionValue, string replacementValue, int columnInfoID, Dictionary<ColumnInfo,string> pkValues)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object> {
                {"RedactionConfiguration_ID",redactionConfigurationID},
                {"StartingIndex",startingIndex},
                {"RedactedValue", redactionValue },
                {"ReplacementValue", replacementValue },
                {"ColumnInfo_ID", columnInfoID }
            });
            foreach (var tuple in pkValues)
            {
                var key = new RegexRedactionKey(repository, this, tuple.Key, tuple.Value);
                key.SaveToDatabase();
            }
        }

        internal RegexRedaction(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            _redactionConfigurationID = Int32.Parse(r["RedactionConfiguration_ID"].ToString());
            _startingIndex = Int32.Parse(r["StartingIndex"].ToString());
            _replacementValue = r["ReplacementValue"].ToString();
            _redactedValue = r["RedactedValue"].ToString();
            _columnInfoID = Int32.Parse(r["ColumnInfo_ID"].ToString());
        }
    }
}
