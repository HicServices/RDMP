using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.CodeDom;
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
        public CohortAggregateContainer() { }
        public CohortAggregateContainer(RDMPDbContext catalogueDbContext, SetOperation uNION)
        {
            CatalogueDbContext = catalogueDbContext;
        }

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

        [NotMapped]
        public virtual List<CohortaggregateSubContainer> SubContainers { get; set; }

        [NotMapped]
        public virtual CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }

        public IMapsDirectlyToDatabaseTable Child => throw new NotImplementedException();
        [NotMapped]
        public int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public CancellationToken CancellationToken { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [NotMapped]
        public CancellationTokenSource CancellationTokenSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public CompilationState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public Exception CrashMessage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public string Log { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public int FinalRowCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public int? CumulativeRowCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public Stopwatch Stopwatch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [NotMapped]
        public TimeSpan? ElapsedTime => throw new NotImplementedException();

        [NotMapped]
        public CohortAggregateContainer RootCohortAggregateContainer { get; internal set; }

        [NotMapped]
        public Catalogue Catalogue => throw new NotImplementedException();

        public int? QueryCachingServer_ID { get; set; }

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

        public void SetKnownContainer(CohortAggregateContainer parent, bool isFirstInContainer)
        {
            throw new NotImplementedException();
        }

        public List<CohortAggregateContainer> GetAllParentContainers() => new List<CohortAggregateContainer>() { };

        public CohortAggregateContainer GetParentContainerIfAny() => null;


        internal void RemoveChild(AggregateConfiguration sourceAggregate)
        {
            throw new NotImplementedException();
        }

        public CohortAggregateContainer GetCohortIdentificationConfiguration()
        {
            throw new NotImplementedException();
        }

        public bool ShouldBeReadOnly(string name, out string reason)
        {
            throw new NotImplementedException();
        }

        internal ISqlParameter[] GetAllParameters()
        {
            throw new NotImplementedException();
        }

        internal void MakeIntoAnOrphan()
        {
            throw new NotImplementedException();
        }

        public void AddChild(AggregateConfiguration configuration, int order) { }
        public void AddChild(CohortAggregateContainer configuration) { }

        internal void AddChild(AggregateConfiguration aggregate, CohortAggregateContainer srcContainer)
        {
            throw new NotImplementedException();
        }

        internal bool IsRootContainer()
        {
            throw new NotImplementedException();
        }

        internal List<CohortaggregateSubContainer> GetSubContainers()
        {
            throw new NotImplementedException();
        }

        internal CohortAggregateContainer CreateClone(ThrowImmediatelyCheckNotifier quiet, object value)
        {
            throw new NotImplementedException();
        }

        public void CreateInsertionPointAtOrder(IOrderable source, int targetOrder, bool v)
        {
            throw new NotImplementedException();
        }
    }
}
