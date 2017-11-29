using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using RDMPStartup;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    public class FilterMenu : RDMPContextMenuStrip
    {
        public FilterMenu(RDMPContextMenuStripArgs args, IFilter filter): base(args, (DatabaseEntity)filter)
        {
            var cata = filter.GetCatalogue();

            var columnInfo = filter.GetColumnInfoIfExists();

            if (columnInfo != null)
            {
                Items.Add("View Extract", GetImage(RDMPConcept.TableInfo,OverlayKind.Filter), (s, e) => _activator.ViewDataSample(new ViewTableInfoExtractUICollection(columnInfo.TableInfo, ViewType.TOP_100, filter)));
                Items.Add("View Extract (" + columnInfo.GetRuntimeName() + ")", GetImage(RDMPConcept.ColumnInfo, OverlayKind.Filter), (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(columnInfo, ViewType.TOP_100, filter)));
                //create right click context menu
                Items.Add("View Aggreggate (of " + columnInfo.GetRuntimeName() + ")", GetImage(RDMPConcept.ColumnInfo, OverlayKind.Filter), (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(columnInfo, ViewType.Aggregate, filter)));

            }
            
            if (cata != null)
            { 
                //compatible graphs are those that are not part of a cic (i.e. they are proper aggregate graphs)
                var compatibleGraphs = cata.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();

                if (compatibleGraphs.Any())
                {
                    var graphMenu = new ToolStripMenuItem("View Aggregate Graph of Filter",GetImage(RDMPConcept.AggregateGraph));

                    foreach (AggregateConfiguration graph in compatibleGraphs)
                    {
                        var collection = new FilterGraphObjectCollection(graph, (ConcreteFilter)filter);
                        graphMenu.DropDownItems.Add(graph.Name,GetImage(RDMPConcept.AggregateGraph, OverlayKind.Filter),(s, e) => _activator.ViewFilterGraph(this, collection));
                    }
                    Items.Add(graphMenu);
                }
            }
        }
    }
}