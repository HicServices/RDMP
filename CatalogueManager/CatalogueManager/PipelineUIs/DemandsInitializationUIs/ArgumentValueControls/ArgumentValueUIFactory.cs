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
        public IArgumentValueUI Create(IArgumentHost parent, IArgument argument, RequiredPropertyInfo required, DataTable previewIfAny, Action<object> setterAction, Action<Exception> fatalAction)
        {
            var argumentType = argument.GetSystemType();
            IArgumentValueUI toReturn;
            var catalogueRepository = (CatalogueRepository)argument.Repository;
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
                        toReturn = new ArgumentValuePipelineUI(catalogueRepository, parent, argumentType);
                    else if (typeof (bool) == argumentType)
                        toReturn = new ArgumentValueBoolUI();
                    else if (required.Demand.DemandType == DemandType.SQL) //if it is SQL
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
                        if (required.Demand.TypeOf == null)
                            throw new NotSupportedException("Property " + argument.Name + " has Property Type '" +
                                                            argumentType +
                                                            "' but does not have a TypeOf specified (e.g. [DemandsInitialization(\"some desc\",DemandType.Unspecified,null,typeof(IDilutionOperation))]).  Without the typeof(X) we do not know what Types to advertise as selectable to the user");

                        toReturn =
                            new ArgumentValueComboBoxUI(
                                catalogueRepository.MEF.GetAllTypes()
                                    .Where(t => required.Demand.TypeOf.IsAssignableFrom(t))
                                    .ToArray());
                    }
                    else if (typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(argumentType))
                    {
                        toReturn = HandleCreateForIMapsDirectlyToDatabaseTable(argument, catalogueRepository, parent,
                            argumentType,
                            true);
                    }
                    else if (typeof (Enum).IsAssignableFrom(argument.GetSystemType()))
                    {
                        toReturn =
                            new ArgumentValueComboBoxUI(
                                Enum.GetValues(argument.GetSystemType()).Cast<object>().ToArray());
                    }
                    else if (typeof (CatalogueRepository).IsAssignableFrom(argumentType))
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
                throw new Exception("A problem occured trying to create an ArgumentUI for Property '" + argument.Name + "' of Type '" + argument.Type +"' on parent class of Type '" + parent.GetClassNameWhoArgumentsAreFor() +"'",e);
            }

            ((Control)toReturn).Dock = DockStyle.Fill;

            var args = new ArgumentValueUIArgs();
            args.InitialValue = argument.GetValueAsSystemType();
            args.Type = argument.GetSystemType();
            args.Required = required;
            args.PreviewIfAny = previewIfAny;
            args.CatalogueRepository = catalogueRepository;
            args.Setter = setterAction;
            args.Fatal = fatalAction;

            toReturn.SetUp(args);
            return toReturn;
        }

        public IArgumentValueUI HandleCreateForIMapsDirectlyToDatabaseTable(IArgument argument, CatalogueRepository repository, IArgumentHost parent, Type argumentType, bool relatedOnlyToLoadMetadata)
        {
            //value is in IMapsDirectly type e.g. .Catalogue/TableInfo or something
            object[] array;
            
            argumentType = argument.GetConcreteSystemType();

            //Populate dropdown with the appropriate types
            if (argumentType == typeof(TableInfo))
                array = GetAllTableInfosAssociatedWithLoadMetadata(repository, parent).ToArray(); //explicit cases where selection is constrained somehow
            else if (argumentType == typeof (ColumnInfo))
                array = GetAdvertisedColumnInfos(repository,parent,relatedOnlyToLoadMetadata);
            else if (argumentType == typeof (PreLoadDiscardedColumn))
                array = GetAllPreloadDiscardedColumnsAssociatedWithLoadMetadata(repository, parent).ToArray();
            else if (argumentType == typeof (LoadProgress))
                array = GetAllLoadProgressAssociatedWithLoadMetadata(parent).ToArray();
            else
                array = repository.GetAllObjects(argumentType).ToArray(); //Default case fetch all the objects of the Type

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


        private object[] GetAdvertisedColumnInfos(CatalogueRepository repository,IArgumentHost parent, bool relatedOnlyToLoadMetadata)
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
