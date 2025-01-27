using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.ReusableLibraryCode;

internal interface IVersionable
{

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    DatabaseEntity SaveNewVersion();
}
