// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Handles creating instances of the correct implementation of <see cref="IArgumentValueUI"/> based on the Type
/// of property being edited <see cref="ArgumentValueUIArgs"/>
/// </summary>
public class ArgumentValueUIFactory
{
    private IActivateItems _activator;

    public IArgumentValueUI Create(IActivateItems activator,ArgumentValueUIArgs args)
    {
        _activator = activator;
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
                toReturn = new ArgumentValueArrayUI(activator);
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
                        $"Demanded type (of DemandsInitialization) was DemandType.SQL but the ProcessTaskArgument Property was of type {argumentType} (Expected String)");

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
                    throw new NotSupportedException(
                        $"Property {args.Required.Name} has Property Type '{argumentType}' but does not have a TypeOf specified (e.g. [DemandsInitialization(\"some desc\",DemandType.Unspecified,null,typeof(IDilutionOperation))]).  Without the typeof(X) we do not know what Types to advertise as selectable to the user");

                toReturn =
                    new ArgumentValueComboBoxUI(activator,
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
                    new ArgumentValueComboBoxUI(activator,
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
            throw new Exception(
                $"A problem occured trying to create an ArgumentUI for Property '{args.Required.Name}' of Type '{argumentType}' on parent class of Type '{args.Parent.GetClassNameWhoArgumentsAreFor()}'", e);
        }

        ((Control)toReturn).Dock = DockStyle.Fill;
            
        toReturn.SetUp(activator, args);
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
            var implmenetationType = args.CatalogueRepository.MEF.GetType(args.Type.Name[1..]);
            if (implmenetationType != null)
                argumentType = implmenetationType;
        }

        //Populate dropdown with the appropriate types
        if (argumentType == typeof(TableInfo))
            array = GetTableInfosInScope(args.CatalogueRepository, args.Parent).ToArray(); //explicit cases where selection is constrained somehow
        else if (argumentType == typeof (ColumnInfo))
            array = GetColumnInfosInScope(args.CatalogueRepository, args.Parent).ToArray();
        else if (argumentType == typeof (PreLoadDiscardedColumn))
            array = GetAllPreloadDiscardedColumnsInScope(args.CatalogueRepository, args.Parent).ToArray();
        else if (argumentType == typeof (LoadProgress) && args.Parent is ProcessTask pt)
            array = pt.LoadMetadata.LoadProgresses;
        else
            array = args.CatalogueRepository.GetAllObjects(argumentType).ToArray(); //Default case fetch all the objects of the Type

        return new ArgumentValueComboBoxUI(_activator,array);
    }

    private IEnumerable<TableInfo> GetTableInfosInScope(ICatalogueRepository repository, IArgumentHost parent)
    {
        if(parent is ProcessTask pt)
            return pt.GetTableInfos();

        if(parent is LoadMetadata lmd)
            return lmd.GetDistinctTableInfoList(true);

        return repository.GetAllObjects<TableInfo>();
    }

        
    private IEnumerable<ColumnInfo> GetColumnInfosInScope(ICatalogueRepository repository,IArgumentHost parent)
    {
        if(parent is ProcessTask || parent is LoadMetadata)
            return GetTableInfosInScope(repository,parent).SelectMany(ti => ti.ColumnInfos);
            
        return repository.GetAllObjects<ColumnInfo>();
    }
        
    private IEnumerable<PreLoadDiscardedColumn> GetAllPreloadDiscardedColumnsInScope(ICatalogueRepository repository, IArgumentHost parent)
    {
        if(parent is ProcessTask || parent is LoadMetadata)
            return GetTableInfosInScope(repository, parent).SelectMany(t => t.PreLoadDiscardedColumns);

        return repository.GetAllObjects<PreLoadDiscardedColumn>();
    }

    /// <summary>
    /// Returns true if the <see cref="IArgumentValueUI"/> for the given <paramref name="argsType"/> supports being
    /// sent illegal string values (e.g. "fish" for typeof(int)).
    /// </summary>
    /// <param name="argsType"></param>
    /// <returns></returns>
    public bool CanHandleInvalidStringData(Type argsType)
    {
        return argsType.IsValueType && !typeof(bool).IsAssignableFrom(argsType)&& !typeof(Enum).IsAssignableFrom(argsType);
    }
}