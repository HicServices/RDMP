// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    public class ArgumentValueUIFactory
    {
        public IArgumentValueUI Create(ArgumentValueUIArgs args)
        {
            var argumentType = args.Type;
            IArgumentValueUI toReturn;
            var catalogueRepository = args.CatalogueRepository;
            try
            {
                //if it's an array
                if(typeof(IDictionary).IsAssignableFrom(argumentType))
                    toReturn = new ArgumentValueDictionaryUI();
                else
                if (typeof (Array).IsAssignableFrom(argumentType))
                    toReturn = new ArgumentValueArrayUI();
                else
                    //if it's a pipeline
                    if (typeof (IPipeline).IsAssignableFrom(argumentType))
                        toReturn = new ArgumentValuePipelineUI(catalogueRepository, args.Parent, argumentType);
                    else if (typeof (bool) == argumentType)
                        toReturn = new ArgumentValueBoolUI();
                    else if (args.Required.Demand.DemandType == DemandType.SQL) //if it is SQL
                    {
                        if (typeof (string) != argumentType)
                            throw new NotSupportedException(
                                "Demanded type (of DemandsInitialization) was DemandType.SQL but the ProcessTaskArgument Property was of type " +
                                argumentType + " (Expected String)");

                        toReturn = new ArgumentValueSqlUI();
                    }
                    else if (typeof (ICustomUIDrivenClass).IsAssignableFrom(argumentType))
                    {
                        toReturn = new ArgumentValueCustomUIDrivenClassUI();
                    }
                    else if (argumentType == typeof (Type))
                    {
                        //Handle case where Demand is for the user to pick a Type (derived from a given parent Type/Interface).  Use case for this is when you want them to pick e.g. a IDilutionOperation where these are a list of classes corrupt data to greater or lesser degree and can be plugin Types but all share the same parent interface IDilutionOperation

                        //There must be a shared parent Type for the user to  pick from
                        if (args.Required.Demand.TypeOf == null)
                            throw new NotSupportedException("Property " + args.Required.Name + " has Property Type '" +
                                                            argumentType +
                                                            "' but does not have a TypeOf specified (e.g. [DemandsInitialization(\"some desc\",DemandType.Unspecified,null,typeof(IDilutionOperation))]).  Without the typeof(X) we do not know what Types to advertise as selectable to the user");

                        toReturn =
                            new ArgumentValueComboBoxUI(
                                catalogueRepository.MEF.GetAllTypes()
                                    .Where(t => args.Required.Demand.TypeOf.IsAssignableFrom(t))
                                    .ToArray());
                    }
                    else if (typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(argumentType))
                    {
                        toReturn = HandleCreateForIMapsDirectlyToDatabaseTable(args);
                    }
                    else if (typeof(Enum).IsAssignableFrom(argumentType))
                    {
                        toReturn =
                            new ArgumentValueComboBoxUI(
                                Enum.GetValues(argumentType).Cast<object>().ToArray());
                    }
                    else if (typeof (ICatalogueRepository).IsAssignableFrom(argumentType))
                    {
                        toReturn = new ArgumentValueLabelUI("<this value cannot be set manually>");
                    }
                    else //type is simple
                    {
                        toReturn =
                            new ArgumentValueTextUI(isPassword: typeof (IEncryptedString).IsAssignableFrom(argumentType));
                    }
            }
            catch (Exception e)
            {
                throw new Exception("A problem occured trying to create an ArgumentUI for Property '" + args.Required.Name + "' of Type '" + argumentType + "' on parent class of Type '" + args.Parent.GetClassNameWhoArgumentsAreFor() + "'", e);
            }

            ((Control)toReturn).Dock = DockStyle.Fill;
            
            toReturn.SetUp(args);
            return toReturn;
        }

        public IArgumentValueUI HandleCreateForIMapsDirectlyToDatabaseTable(ArgumentValueUIArgs args)
        {
            //value is in IMapsDirectly type e.g. .Catalogue/TableInfo or something
            object[] array;

            var argumentType = args.Type;

            //if it is an interface e.g. IExternalDatabaseServer look for ExternalDatabaseServer
            if (argumentType.IsInterface)
            {
                var implmenetationType = args.CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(args.Type.Name.Substring(1));
                if (implmenetationType != null)
                    argumentType = implmenetationType;
            }

            //Populate dropdown with the appropriate types
            if (argumentType == typeof(TableInfo))
                array = GetAllTableInfosAssociatedWithLoadMetadata(args.CatalogueRepository, args.Parent).ToArray(); //explicit cases where selection is constrained somehow
            else if (argumentType == typeof (ColumnInfo))
                array = GetAdvertisedColumnInfos(args.CatalogueRepository, args.Parent, true);
            else if (argumentType == typeof (PreLoadDiscardedColumn))
                array = GetAllPreloadDiscardedColumnsAssociatedWithLoadMetadata(args.CatalogueRepository, args.Parent).ToArray();
            else if (argumentType == typeof (LoadProgress))
                array = GetAllLoadProgressAssociatedWithLoadMetadata(args.Parent).ToArray();
            else
                array = args.CatalogueRepository.GetAllObjects(argumentType).ToArray(); //Default case fetch all the objects of the Type

            return new ArgumentValueComboBoxUI(array);
        }

        private IEnumerable<TableInfo> GetTableInfosFromParentOrThrow(ICatalogueRepository repository, IArgumentHost parent)
        {
            var t = parent as ITableInfoCollectionHost;

            if (t == null)
                return repository.GetAllObjects<TableInfo>();

            return t.GetTableInfos();
        }

        private IEnumerable<TableInfo> GetAllTableInfosAssociatedWithLoadMetadata(ICatalogueRepository  repository,IArgumentHost parent)
        {
            return GetTableInfosFromParentOrThrow(repository,parent);
        }


        private object[] GetAdvertisedColumnInfos(ICatalogueRepository repository,IArgumentHost parent, bool relatedOnlyToLoadMetadata)
        {
            return relatedOnlyToLoadMetadata
                ? GetAllColumnInfosAssociatedWithLoadMetadata(repository, parent).ToArray()
                : repository.GetAllObjects<ColumnInfo>().ToArray();
        }


        private IEnumerable<ColumnInfo> GetAllColumnInfosAssociatedWithLoadMetadata(ICatalogueRepository repository,IArgumentHost parent)
        {
            return GetTableInfosFromParentOrThrow(repository,parent).SelectMany(ti => ti.ColumnInfos);
        }
        
        private IEnumerable<PreLoadDiscardedColumn> GetAllPreloadDiscardedColumnsAssociatedWithLoadMetadata(ICatalogueRepository repository, IArgumentHost parent)
        {
            return GetTableInfosFromParentOrThrow(repository, parent).SelectMany(t => t.PreLoadDiscardedColumns);
        }
        private IEnumerable<ILoadProgress> GetAllLoadProgressAssociatedWithLoadMetadata(IArgumentHost parent)
        {
            var h = parent as ILoadProgressHost;

            if (h != null)
                return h.LoadProgresses;

            throw new NotSupportedException("Cannot populate LoadProgress selection list because type " + parent.GetType().Name + " does not support the " + typeof(ILoadProgressHost).Name + " interface");

        }
    }
}
