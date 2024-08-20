using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public interface IRegexRedaction
    {

        int RedactionConfiguration_ID { get; protected set; }
        string RedactedValue { get; protected set; }
        string ReplacementValue { get; protected set; }

        int startingIndex { get; protected set; }
    }
}
