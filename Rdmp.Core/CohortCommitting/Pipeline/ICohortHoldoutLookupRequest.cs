using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CohortCommitting.Pipeline;

public interface ICohortHoldoutLookupRequest : ICheckable, IHasDesignTimeMode, IPipelineUseCase
{

}

