using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("CohortAggregateContainer")]
    public class CohortAggregateContainer : DatabaseObject, ICompileable
    {
        [Key]
        public override int ID { get; set; }

        public int Order { get; set => SetField(ref field, value); }
        public string Operation { get; set => SetField(ref field, value); }
        public string Name { get; set => SetField(ref field, value); }
        public bool IsDisabled { get; set => SetField(ref field, value); }

        public event EventHandler StateChanged
        {
            add { throw new NotSupportedException(); }
            remove { throw new NotSupportedException(); }
        }


        public override string ToString() => Name;

        public virtual List<CohortaggregateSubContainer> SubContainers { get; set; }

        public virtual CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }

        public IMapsDirectlyToDatabaseTable Child => throw new NotImplementedException();

        public int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CancellationToken CancellationToken { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CancellationTokenSource CancellationTokenSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CompilationState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Exception CrashMessage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Log { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int FinalRowCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? CumulativeRowCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Stopwatch Stopwatch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TimeSpan? ElapsedTime => throw new NotImplementedException();

        public List<object> GetOrderedContents()
        {
            return new List<object>(); //TODO
        }

        public List<CohortAggregateContainer> GetAllSubContainersRecursively()
        {
            return new List<CohortAggregateContainer>(); //TODO
        }
        public List<CohortAggregateContainer> GetAggregateConfigurations()//TODO this is the wrong kind
        {
            return new List<CohortAggregateContainer>(); //TODO
        }

        public List<AggregateConfiguration> GetAllAggregateConfigurationsRecursively()
        {
            return new List<AggregateConfiguration>();
        }

        public IDataAccessPoint[] GetDataAccessPoints()
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled()
        {
            throw new NotImplementedException();
        }

        public string GetCachedQueryUseCount()
        {
            throw new NotImplementedException();
        }

        public void SetKnownContainer(Curation.Data.Cohort.CohortAggregateContainer parent, bool isFirstInContainer)
        {
            throw new NotImplementedException();
        }
    }
}
