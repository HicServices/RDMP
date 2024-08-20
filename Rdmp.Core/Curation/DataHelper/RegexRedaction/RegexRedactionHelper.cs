using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public static class RegexRedactionHelper
    {

        public static string ConvertPotentialDateTimeObject(string value, string currentColumnType)
        {
            var matchValue = $"'{value}'";
            if (currentColumnType == "datetime2" || currentColumnType == "datetime")
            {
                var x = DateTime.Parse(value);
                var format = "yyyy-MM-dd HH:mm:ss:fff";
                matchValue = $"'{x.ToString(format)}'";
            }
            return matchValue;
        }
    }
}
