using System.Linq;
using CatalogueManager.Collections;
using CatalogueManager.SimpleDialogs.Reports;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// A Document Tab that hosts an RDMPCollection, the control knows how to save itself to the persistence settings file for the user ensuring that when they next open the
    /// software the Tab can be reloaded and displayed.  Persistance involves storing this Tab type, the Collection Control type being hosted by the Tab (an RDMPCollection).
    /// Since there can only ever be one RDMPCollection of any Type active at a time this is all that must be stored to persist the control
    /// </summary>
    [TechnicalUI]
    [System.ComponentModel.DesignerCategory("")]
    public class PersistableToolboxDockContent:DockContent
    {
        public const string Prefix = "Toolbox";

        public readonly RDMPCollection CollectionType;

        public PersistableToolboxDockContent(RDMPCollection collectionType)
        {
            CollectionType = collectionType;
        }

        protected override string GetPersistString()
        {
            return Prefix + PersistenceDecisionFactory.Separator + CollectionType;
        }

        public RDMPCollectionUI GetCollection()
        {
            return Controls.OfType<RDMPCollectionUI>().SingleOrDefault();
        }
    }
}
