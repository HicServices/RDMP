// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
        public virtual bool IsInteractive => true;

        /// <inheritdoc/>
        public ICoreChildProvider CoreChildProvider { get; protected set;}

        /// <inheritdoc/>
        public IServerDefaults ServerDefaults { get; }

        /// <inheritdoc/>
        public abstract bool YesNo(string text, string caption, out bool chosen);

        /// <inheritdoc/>
        public bool YesNo(string text, string caption)
        {
            return YesNo(text, caption, out bool chosen) && chosen;
        }

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

            // Note that this is virtual so can return null e.g. if other stuff has to happen with the activator before a valid child provider can be built (e.g. loading plugin user interfaces)
            CoreChildProvider = GetChildProvider();
        }

        protected virtual ICoreChildProvider GetChildProvider()
        {
            return RepositoryLocator.DataExportRepository != null?
                            new DataExportChildProvider(RepositoryLocator,null,GlobalErrorCheckNotifier, CoreChildProvider as DataExportChildProvider):
                            new CatalogueChildProvider(RepositoryLocator.CatalogueRepository,null,GlobalErrorCheckNotifier,CoreChildProvider as CatalogueChildProvider);
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

        /// <summary>
        /// Returns the root tree object which hosts the supplied object.  If the supplied object has no known descendancy it is assumed
        /// to be the root object itself so it is returned
        /// </summary>
        /// <param name="objectToEmphasise"></param>
        /// <returns></returns>
        public object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise)
        {
            return CoreChildProvider?.GetRootObjectOrSelf(objectToEmphasise) ?? objectToEmphasise;
        }

        public abstract bool SelectEnum(string prompt, Type enumType, out Enum chosen);
        public virtual bool SelectType(string prompt, Type baseTypeIfAny, out Type chosen)
        {
            Type[] available =
            RepositoryLocator.CatalogueRepository.MEF.GetAllTypes()
                .Where(t => baseTypeIfAny == null || baseTypeIfAny.IsAssignableFrom(t))
                .ToArray();

            return SelectType(prompt, available, out chosen);
        }

        public abstract bool SelectType(string prompt, Type[] available, out Type chosen);
        
        public virtual void Activate(DatabaseEntity o)
        {
            
        }

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
        public virtual bool DeleteWithConfirmation(IDeleteable deleteable)
        {
            deleteable.DeleteInDatabase();

            if(deleteable is DatabaseEntity d)
                Publish(d);

            return true;
        }

        /// <inheritdoc/>
        public bool SelectValueType(string prompt, Type paramType, object initialValue, out object chosen)
        {

            if ((Nullable.GetUnderlyingType(paramType) ?? paramType).IsEnum)
            {
                bool ok = SelectEnum(prompt, paramType, out Enum enumChosen);
                chosen = enumChosen;
                return ok;
            }

            if (paramType == typeof(bool) || paramType == typeof(bool?))
            {
                bool ok = YesNo(prompt, "Enter Value", out bool boolChosen);
                chosen = boolChosen;
                return ok;
            }

            if (paramType == typeof(string))
            {
                bool ok = TypeText("Enter Value",prompt,int.MaxValue, initialValue?.ToString(),out string stringChosen,false);
                chosen = stringChosen;
                return ok;

            }

            return SelectValueTypeImpl(prompt, paramType, initialValue, out chosen);
        }

        protected abstract bool SelectValueTypeImpl(string prompt, Type paramType, object initialValue, out object chosen);

        /// <inheritdoc/>
        public virtual void Publish(DatabaseEntity databaseEntity)
        {
            var fresh = GetChildProvider();
            CoreChildProvider.UpdateTo(fresh);
        }

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
        public abstract FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern);

        /// <inheritdoc/>
        public virtual List<CommandInvokerDelegate> GetDelegates()
        {
            return new List<CommandInvokerDelegate>();
        }

    }
}