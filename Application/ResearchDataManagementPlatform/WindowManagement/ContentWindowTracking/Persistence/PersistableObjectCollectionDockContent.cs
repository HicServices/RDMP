using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableUIComponents;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// Allows you to persist user interfaces which are built on more than one RDMP database object (if you only require one object you should use RDMPSingleDatabaseObjectControl instead
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    [TechnicalUI]
    public class PersistableObjectCollectionDockContent : RDMPSingleControlTab
    {
        private readonly IObjectCollectionControl _control;
        
        public const string Prefix = "RDMPObjectCollection";
        public const string ExtraText = "###EXTRA_TEXT###";
        public const string CollectionStartDelimiter = "[";
        public const string CollectionEndDelimiter = "]";
        public const string CollectionObjectSeparator = ",";

        public PersistableObjectCollectionDockContent(IActivateItems activator, IObjectCollectionControl control, IPersistableObjectCollection collection):base(activator.RefreshBus)
        {
            _control = control;
            
            //tell the control what it's collection is
            control.SetCollection(activator, collection);
            
            //ask the control what it wants its name to be
            TabText = _control.GetTabName();
        }

        protected override string GetPersistString()
        {
            var collection = _control.GetCollection();
            const char s = PersistenceDecisionFactory.Separator;

            //Looks something like this  RDMPObjectCollection:MyCoolControlUI:MyControlUIsBundleOfObjects:[CatalogueRepository:AggregateConfiguration:105,CatalogueRepository:AggregateConfiguration:102,CatalogueRepository:AggregateConfiguration:101]###EXTRA_TEXT###I've got a lovely bunch of coconuts
            StringBuilder sb = new StringBuilder();

            //Output <Prefix>:<The Control Type>:<The Type name of the Collection - must be new()>:
            sb.Append(Prefix + s + _control.GetType().FullName + s  + collection.GetType().Name + s);

            //output [obj1,obj2,obj3]
            sb.Append(CollectionStartDelimiter);

            //where obj is <RepositoryType>:<DatabaseObjectType>:<ObjectID>
            sb.Append(string.Join(CollectionObjectSeparator, collection.DatabaseObjects.Select(o => o.Repository.GetType().FullName + s + o.GetType().FullName + s + o.ID)));

            //ending bracket for the object collection
            sb.Append(CollectionEndDelimiter);

            //now add the bit that starts the user specific text
            sb.Append(ExtraText);

            //let him save whatever text he wants
            sb.Append(collection.SaveExtraText());

            return sb.ToString();
        }

        public static string MatchCollectionInString(string persistenceString)
        {
            try
            {
                //match the starting delimiter
                string pattern = Regex.Escape(CollectionStartDelimiter);
                pattern += "(.*)";//then anything
                pattern += Regex.Escape(CollectionEndDelimiter);//then the ending delimiter

                return Regex.Match(persistenceString, pattern).Groups[1].Value;
            }
            catch (Exception e)
            {
                throw new PersistenceException("Could not match ObjectCollection delimiters in persistenceString '" + persistenceString + "'",e);
            }
        }

        public override Control GetControl()
        {
            return (Control) _control;
        }

        public override void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            TabText = _control.GetTabName();

            //pass the info on to the control
            _control.RefreshBus_RefreshObject(sender,e);

        }

        public override void HandleUserRequestingTabRefresh(IActivateItems activator)
        {
            var collection = _control.GetCollection();

            foreach (var o in collection.DatabaseObjects)
            {
                var revertable = o as IRevertable;
                if (revertable != null)
                    revertable.RevertToDatabaseState();
            }

            _control.SetCollection(activator,collection);
        }


        public override void HandleUserRequestingEmphasis(IActivateItems activator)
        {
            var collection = _control.GetCollection();

            if(collection != null)
                if(collection.DatabaseObjects.Count == 1)
                    activator.RequestItemEmphasis(this, new EmphasiseRequest(collection.DatabaseObjects[0]));
        }
    }
}
