using System;
using System.Diagnostics;
using System.Threading;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.Execution;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CohortManagerLibrary
{
    public abstract class Compileable:ICompileable
    {
        protected readonly CohortCompiler _compiler;
        private CompilationState _state;
        
        public CohortAggregateContainer ParentContainerIfAny { get; set; }
        public bool? IsFirstInContainer { get; set; }

        protected Compileable(CohortCompiler compiler)
        {
            _compiler = compiler;
        }

        public override string ToString()
        {
            return Child.ToString();
        }

        public string GetStateDescription()
        {
            return State.ToString();
        }

        public abstract string GetCatalogueName();

        public CancellationToken CancellationToken { set; get; }

        public CompilationState State
        {
            set
            {
                _state = value;
                var h = StateChanged;
                if(h != null)
                    h(this,new EventArgs());
            }
            get { return _state; }
        }

        public virtual int Order
        {
            get { return ((IOrderable) Child).Order; }
            set { ((IOrderable) Child).Order = value; }
        }

        public event EventHandler StateChanged;
        public Exception CrashMessage { get; set; }
        
        public int FinalRowCount { set; get; }
        
        public int? CumulativeRowCount { set; get; }

        public abstract IMapsDirectlyToDatabaseTable Child { get; }
        public int Timeout { get; set; }
        public abstract IDataAccessPoint[] GetDataAccessPoints();


        public Stopwatch Stopwatch { get; set; }

        public TimeSpan? ElapsedTime {
            get
            {
                if (Stopwatch == null)
                    return null;

                return Stopwatch.Elapsed;
            }
        }

        public string GetCachedQueryUseCount()
        {
           return _compiler.GetCachedQueryUseCount(this);
        }

        public bool AreaAllQueriesCached()
        {
            return _compiler.AreaAllQueriesCached(this);
        }
        public void SetKnownContainer(CohortAggregateContainer parent, bool isFirstInContainer)
        {
            ParentContainerIfAny = parent;
            IsFirstInContainer = isFirstInContainer;
        }
    }
}