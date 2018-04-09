using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    /// <summary>
    /// Used to model Runtime initialization.  IDataFlowComponents which implement this interface (you can implement for multiple T) will be Initialized with a compatible Initialization
    ///  Object available in the IPipelineUseCase.
    /// 
    /// <para>It specifies that an IDataFlowComponent or IDataFlowSource requires a particular object at execution time to function properly e.g.  a source which extractes linked data
    /// for a cohort might require an ExtractionRequest object (which must be provided by the hosting environment).  You can only currently have 1 object of each type.  </para>
    /// 
    /// <para>IMPORTANT: If you can store the value you require Immutably instead e.g. Extraction Format CSV/TSV then you should instead use a [DemandsInitialization].  The difference
    /// between this and DemandsInitialization is that this interface is dynamic and dependent on who is executing it and what they are executing it on while DemandsInitialization 
    /// is configured once in the Catalogue (although it can be changed later) and is a constant at construction time.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipelineRequirement<in T>
    {
        /// <summary>
        /// Initializes your object with some object of type T that corresponds to the usage context you are about to be executed under.  You can implement multiple copies of this 
        /// interface if you need for example an ExtractionRequest and an AuditObject and a EmailAddressOfAuthorizor or something.
        /// 
        /// <para>IMPORTANT: You might be being checked and not actually run so when implementing this method you should not make any system changes or advanced auditing stuff.</para>
        /// </summary>
        /// <param name="value">An object</param>
        /// <param name="listener"></param>
        void PreInitialize(T value, IDataLoadEventListener listener);
    }
}
