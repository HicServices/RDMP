using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution
{
    public abstract class BasicActivateItems : IBasicActivateItems
    {
        /// <inheritdoc/>
        public ICoreChildProvider CoreChildProvider { get; protected set; }

        /// <inheritdoc/>
        public IServerDefaults ServerDefaults { get; }

        public abstract bool YesNo(string text, string caption);

        /// <inheritdoc/>
        public ICheckNotifier GlobalErrorCheckNotifier { get; set; }

        /// <inheritdoc/>
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <inheritdoc/>
        public event EmphasiseItemHandler Emphasise;

        public BasicActivateItems(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
        {
            RepositoryLocator = repositoryLocator;
            GlobalErrorCheckNotifier = globalErrorCheckNotifier;

            ServerDefaults = RepositoryLocator.CatalogueRepository.GetServerDefaults();
        }
        
        protected void OnEmphasise(object sender, EmphasiseEventArgs args)
        {
            Emphasise?.Invoke(sender,args);
        }
        /// <inheritdoc/>
        public virtual IEnumerable<Type> GetIgnoredCommands()
        {
            return new Type[0];
        }

        /// <inheritdoc/>
        public virtual void Wait(string title, Task task, CancellationTokenSource cts)
        {
            task.Wait(cts.Token);
        }

        public virtual void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest)
        {
            OnEmphasise(sender, new EmphasiseEventArgs(emphasiseRequest));
        }
        public abstract bool SelectEnum(string prompt, Type enumType, out Enum chosen);

        /// <inheritdoc/>
        public virtual IEnumerable<T> GetAll<T>()
        {
            return CoreChildProvider.GetAllSearchables()
                .Keys.OfType<T>();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IMapsDirectlyToDatabaseTable> GetAll(Type t)
        {
            return CoreChildProvider.GetAllSearchables()
                .Keys.Where(t.IsInstanceOfType);
        }
        
        /// <inheritdoc/>
        public abstract void ShowException(string errorText, Exception exception);

        /// <inheritdoc/>
        public abstract bool DeleteWithConfirmation(IDeleteable deleteable);

        /// <inheritdoc/>
        public abstract object SelectValueType(string prompt, Type paramType);
        
        /// <inheritdoc/>
        public abstract void Publish(DatabaseEntity databaseEntity);

        /// <inheritdoc/>
        public abstract void Show(string message);

        /// <inheritdoc/>
        public abstract bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText);

        /// <inheritdoc/>
        public abstract DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription);

        /// <inheritdoc/>
        public abstract DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription);
        
        /// <inheritdoc/>
        public abstract IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null);

        /// <inheritdoc/>
        public abstract IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false);

        /// <inheritdoc/>
        public abstract DirectoryInfo SelectDirectory(string prompt);

        /// <inheritdoc/>
        public abstract FileInfo SelectFile(string prompt);

        /// <inheritdoc/>
        public abstract FileInfo SelectFile(string prompt, string patternDescription, string pattern);

        /// <inheritdoc/>
        public virtual List<KeyValuePair<Type, Func<RequiredArgument, object>>> GetDelegates()
        {
            return new List<KeyValuePair<Type, Func<RequiredArgument, object>>>();
        }
    }
}