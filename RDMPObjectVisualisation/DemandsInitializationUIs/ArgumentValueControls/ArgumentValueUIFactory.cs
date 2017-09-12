using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    public class ArgumentValueUIFactory
    {
        public IArgumentValueUI Create(IArgumentHost parent, IArgument argument, DemandsInitializationAttribute demand, DataTable previewIfAny)
        {
            var argumentType = argument.GetSystemType();
            IArgumentValueUI toReturn;
            var catalogueRepository = (CatalogueRepository)argument.Repository;
            try
            {
                //if it is a bool
                if (typeof (Pipeline).IsAssignableFrom(argumentType))
                    toReturn = new ArgumentValuePipelineUI(catalogueRepository,parent, argumentType);
                else if (typeof (bool) == argumentType)
                    toReturn = new ArgumentValueBoolUI();
                else if (demand.DemandType == DemandType.SQL) //if it is SQL
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
                    if (demand.TypeOf == null)
                        throw new NotSupportedException("Property " + argument.Name + " has Property Type '" +
                                                        argumentType +
                                                        "' but does not have a TypeOf specified (e.g. [DemandsInitialization(\"some desc\",DemandType.Unspecified,null,typeof(IDilutionOperation))]).  Without the typeof(X) we do not know what Types to advertise as selectable to the user");

                    toReturn =
                        new ArgumentValueComboBoxUI(
                            catalogueRepository.MEF.GetAllTypes()
                                .Where(t => demand.TypeOf.IsAssignableFrom(t))
                                .ToArray());
                }
                else if (typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(argumentType))
                {
                    toReturn = HandleCreateForIMapsDirectlyToDatabaseTable(catalogueRepository, parent, argumentType,
                        true);
                }
                else if (typeof (Enum).IsAssignableFrom(argument.GetSystemType()))
                {
                    toReturn =
                        new ArgumentValueComboBoxUI(Enum.GetValues(argument.GetSystemType()).Cast<object>().ToArray(),
                            true);
                }
                else if (typeof (CatalogueRepository).IsAssignableFrom(argumentType))
                {
                    toReturn = new ArgumentValueLabelUI("<this value cannot be set manually>");
                }
                else //type is simple
                {
                    toReturn = new ArgumentValueTextUI();
                }
            }
            catch (Exception e)
            {
                throw new Exception("A problem occured trying to create an ArgumentUI for Property '" + argument.Name + "' of Type '" + argument.Type +"' on parent class of Type '" + parent.GetClassNameWhoArgumentsAreFor() +"'",e);
            }

            ((Control)toReturn).Dock = DockStyle.Fill;
            toReturn.SetUp((Argument)argument, demand, previewIfAny);
            return toReturn;
        }

        public IArgumentValueUI HandleCreateForIMapsDirectlyToDatabaseTable(CatalogueRepository repository, IArgumentHost parent, Type argumentType, bool relatedOnlyToLoadMetadata)
        {
            //value is in IMapsDirectly type e.g. .Catalogue/TableInfo or something
            object[] array;

            //Populate dropdown with the appropriate types
            if (argumentType == typeof(TableInfo))
                array = GetAllTableInfosAssociatedWithLoadMetadata(parent).ToArray(); //explicit cases where selection is constrained somehow
            else if (argumentType == typeof (ColumnInfo))
                array = GetAdvertisedColumnInfos(repository,parent,relatedOnlyToLoadMetadata);
            else if (argumentType == typeof (PreLoadDiscardedColumn))
                array = GetAllPreloadDiscardedColumnsAssociatedWithLoadMetadata(parent).ToArray();
            else if (argumentType == typeof (LoadProgress))
                array = GetAllLoadProgressAssociatedWithLoadMetadata(parent).ToArray();
            else
                array = repository.GetAllObjects(argumentType).ToArray(); //Default case fetch all the objects of the Type


            return new ArgumentValueComboBoxUI(array);
            /*
            //if we have a value it will be an ID
            if (!string.IsNullOrWhiteSpace(argument.Value))
                try
                {
                    if (valueAsSystemType == typeof(ColumnInfo))
                        ProcessColumnInfoArgumentUI(argument);
                    else
                    {
                        //get the value as the object type e.g. Catlogue
                        cbxValue.Text = argument.GetValueAsSystemType().ToString();
                    }
                }
                catch (KeyNotFoundException e)//the object might have been deleted (it's ID is no longer in the table, if so offer to set the param to null for the user)
                {
                    if (
                        MessageBox.Show(e.Message, "Set value for parameter " + argument.Name + " to Null?",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        argument.Value = null;
                        argument.SaveToDatabase();
                        SetProcessTaskArgument(_repository, parent, argument, demand);//relaunch method having fixed the null problem
                        return;
                    }
                }
            */
            
        }

        /*
        /// <summary>
        /// Set the argument UI for a ColumnInfo arg, taking into account the ability to assign either related or global ColumnInfos and setting the checkbox/dropdown contents accordingly.
        /// </summary>
        /// <param name="argument"></param>
        private void ProcessColumnInfoArgumentUI(IArgument argument)
        {
            cbxValue.Text = argument.GetValueAsSystemType().ToString();

            // If the text value hasn't been set for a ColumnInfo, this is potentially because it is a global reference rather than a column info related to this particular LoadMetadata
            if (string.IsNullOrWhiteSpace(cbxValue.Text))
            {
                if (cbRelatedToLoadMetadata.Checked)
                {
                    // The drop-down is only displaying ColumnInfos related to the LoadMetadata, so change the UI to display all ColumnInfos
                    cbRelatedToLoadMetadata.Checked = false;
                    PopulateDropdownWith(GetAdvertisedColumnInfos());

                    cbxValue.Text = argument.GetValueAsSystemType().ToString();
                }

                // If we're here and still haven't set cbxValue.Text then the ColumnInfo isn't being loaded into the drop-down's list for either global or related ColumnInfos
                if (string.IsNullOrWhiteSpace(cbxValue.Text))
                    throw new InvalidOperationException("Cannot find ColumnInfo " + argument.GetValueAsSystemType() +
                                                        " in the global or LoadMetadata-related list of ColumnInfos");
            }
        }*/
        private IEnumerable<TableInfo> GetTableInfosFromParentOrThrow(IArgumentHost parent)
        {
            var t = parent as ITableInfoCollectionHost;

            if (t == null)
                throw new NotSupportedException("We are trying to configure arguments for a " + parent.GetType().Name + " but it does not implement interface " + typeof(ITableInfoCollectionHost).Name + " which means we cannot support DemandsInitializations of types TableInfo or any derrivative objects (ColumnInfo,Prediscarded Columns etc).");

            return t.GetTableInfos();
        }

        private IEnumerable<TableInfo> GetAllTableInfosAssociatedWithLoadMetadata(IArgumentHost parent)
        {
            return GetTableInfosFromParentOrThrow(parent);
        }


        private object[] GetAdvertisedColumnInfos(CatalogueRepository repository,IArgumentHost parent, bool relatedOnlyToLoadMetadata)
        {
            return relatedOnlyToLoadMetadata
                ? GetAllColumnInfosAssociatedWithLoadMetadata(parent).ToArray()
                : repository.GetAllObjects<ColumnInfo>().ToArray();
        }


        private IEnumerable<ColumnInfo> GetAllColumnInfosAssociatedWithLoadMetadata(IArgumentHost parent)
        {
            return GetTableInfosFromParentOrThrow(parent).SelectMany(ti => ti.ColumnInfos);
        }
      
        
        private IEnumerable<PreLoadDiscardedColumn> GetAllPreloadDiscardedColumnsAssociatedWithLoadMetadata(IArgumentHost parent)
        {
            return GetTableInfosFromParentOrThrow(parent).SelectMany(t=>t.PreLoadDiscardedColumns);
        }
        private IEnumerable<ILoadProgress> GetAllLoadProgressAssociatedWithLoadMetadata(IArgumentHost parent)
        {
            var h = parent as ILoadProgressHost;

            if (h != null)
                return h.GetLoadProgresses();

            throw new NotSupportedException("Cannot populate LoadProgress selection list because type " + parent.GetType().Name + " does not support the " + typeof(ILoadProgressHost).Name + " interface");

        }
    }
}
