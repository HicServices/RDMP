using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public interface IRegexRedactionKey: IMapsDirectlyToDatabaseTable
    {

        int RegexRedaction_ID { get; protected set; }
        int ColumnInfo_ID { get; protected set; }
        string Value { get; protected set; }
    }
}
