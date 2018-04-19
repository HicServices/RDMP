using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Providers;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.FindAndReplace
{
    public partial class FindAndReplaceUI : UserControl
    {
        private readonly IActivateItems _activator;
        private HashSet<IMapsDirectlyToDatabaseTable> _allObjects = new HashSet<IMapsDirectlyToDatabaseTable>();

        private IAttributePropertyFinder _adjustableLocationPropertyFinder;
        private List<FindAndReplaceNode> _locationNodes = new List<FindAndReplaceNode>(); 
        
        private IAttributePropertyFinder _sqlPropertyFinder;
        private List<FindAndReplaceNode> _sqlNodes = new List<FindAndReplaceNode>();

        
        public FindAndReplaceUI(IActivateItems activator)
        {
            _activator = activator;

            GetAllObjects(activator);

            _adjustableLocationPropertyFinder = new AttributePropertyFinder<AdjustableLocationAttribute>(_allObjects);
            _sqlPropertyFinder = new AttributePropertyFinder<SqlAttribute>(_allObjects);

            InitializeComponent();

            olvObject.ImageGetter += ImageGetter;
            olvProperty.AspectGetter += PropertyAspectGetter;
            olvValue.AspectGetter += ValueAspectGetter;
            
            tlvAllObjects.AlwaysGroupByColumn = olvProperty;

            //allow editing
            tlvAllObjects.CellEditFinished += tlvAllObjects_CellEditFinished;

            //Create all the nodes up front
            foreach (IMapsDirectlyToDatabaseTable o in _allObjects.Where(_adjustableLocationPropertyFinder.ObjectContainsProperty))
                foreach (PropertyInfo propertyInfo in _adjustableLocationPropertyFinder.GetProperties(o))
                    _locationNodes.Add( new FindAndReplaceNode(o,propertyInfo));

            foreach (IMapsDirectlyToDatabaseTable o in _allObjects.Where(_sqlPropertyFinder.ObjectContainsProperty))
                foreach (PropertyInfo propertyInfo in _sqlPropertyFinder.GetProperties(o))
                    _sqlNodes.Add(new FindAndReplaceNode(o, propertyInfo));

            tlvAllObjects.AddObjects(_locationNodes);
        }

        private void GetAllObjects(IActivateItems activator)
        {

            Gatherer g = new Gatherer(activator.RepositoryLocator);

            //We get these from the child provider because some objects (those below go off looking stuff up if you get them
            //and do not inject known good values first)
            foreach (var o in _activator.CoreChildProvider.AllExtractionInformations)
                _allObjects.Add(o);

            foreach (var o in _activator.CoreChildProvider.AllCatalogueItems)
                _allObjects.Add(o);

            var dxmChildProvider = _activator.CoreChildProvider as DataExportChildProvider;

            if (dxmChildProvider != null)
                foreach (var o in dxmChildProvider.AllExtractableColumns)
                    _allObjects.Add(o);

            foreach (var o in g.GetAllObjectsInAllDatabases())
                _allObjects.Add(o);
        }

        void tlvAllObjects_CellEditFinished(object sender, CellEditEventArgs e)
        {
            if( e == null || e.RowObject == null)
                return;
            
            var node = (FindAndReplaceNode)e.RowObject;
            node.SetValue(e.NewValue);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs((DatabaseEntity) node.Instance));
        }

        private object ValueAspectGetter(object rowobject)
        {
            return ((FindAndReplaceNode)rowobject).GetCurrentValue();
        }

        private object PropertyAspectGetter(object rowobject)
        {
            var node = ((FindAndReplaceNode) rowobject);

            return node.PropertyName;
        }

        private object ImageGetter(object rowObject)
        {
            return _activator.CoreIconProvider.GetImage(((FindAndReplaceNode) rowObject).Instance);
        }


        private void CheckedChanged(object sender, EventArgs e)
        {
            var cb = (RadioButton)sender;

            if(cb.Checked)
            {
                tlvAllObjects.ClearObjects();

                if (sender == rbLocationsAttribute)
                    tlvAllObjects.AddObjects(_locationNodes);
                else
                    tlvAllObjects.AddObjects(_sqlNodes);
            }
        }

        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            tlvAllObjects.ModelFilter = new TextMatchFilter(tlvAllObjects,tbFind.Text);
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(
                    "Are you sure you want to do a system wide find and replace? This operation cannot be undone",
                    "Are you sure", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (FindAndReplaceNode node in tlvAllObjects.FilteredObjects)
                {
                    node.FindAndReplace(tbFind.Text, tbReplace.Text);

                }
            }
        }

        private void tlvAllObjects_ItemActivate(object sender, EventArgs e)
        {
            var node = tlvAllObjects.SelectedObject as FindAndReplaceNode;

            if (node != null)
            {
                var cmd = new ExecuteCommandActivate(_activator,node.Instance);
                if(!cmd.IsImpossible)
                    cmd.Execute();
            }
        }
    }
}
