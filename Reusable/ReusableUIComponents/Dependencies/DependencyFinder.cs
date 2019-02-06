// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReusableLibraryCode;
using ReusableUIComponents.Dependencies.Models;

namespace ReusableUIComponents.Dependencies
{
    public class DependencyFinder
    {
        private readonly IHasDependencies _root;
        private readonly int _dependencyDepthToFind;
        private readonly Dictionary<Type, bool> _vertexTypeFilters;
        private readonly Dictionary<string, bool> _vertexHighlightFilters;
        Dictionary<IHasDependencies, DataVertex> dependenciesFoundDuringRecursion = new Dictionary<IHasDependencies, DataVertex>();
        private IObjectVisualisation _visualiser;

        public bool IsWorking { get; set; }
        public bool Cancelled { get; private set; }
        public bool AllowCaching { get; set; }

        public Dictionary<IHasDependencies, IHasDependencies[]> ChildCache { get; private set; }
        public Dictionary<IHasDependencies, IHasDependencies[]> ParentCache { get; private set; }


        public DependencyFinder(IHasDependencies root, int dependencyDepthToFind, Dictionary<Type, bool> vertexTypeFilters,
            Dictionary<IHasDependencies, IHasDependencies[]> _parentCache, Dictionary<IHasDependencies, IHasDependencies[]> _childCache, IObjectVisualisation visualiser, Dictionary<string, bool> vertexHighlightFilters)
        {
            _root = root;
            _dependencyDepthToFind = dependencyDepthToFind;
            _vertexHighlightFilters = vertexHighlightFilters;
            _vertexTypeFilters = vertexTypeFilters;
            ChildCache = _childCache;
            ParentCache = _parentCache;
            _visualiser = visualiser;
            AllowCaching = true;
        }


        private void AddDependenciesToGraphRecursively(IHasDependencies currentNode, DataVertex currentNodeAsVertex, GraphExample dataGraph, int depth)
        {
            if (Cancelled)
                return;

            //show up to this depth
            if (depth > _dependencyDepthToFind)
                return;

            //objects on which this depends
            IHasDependencies[] children = GetChildrenWithCache(currentNode); ;

            //objects this depends upon (e.g. a ColumnInfo depends upon a TableInfo)
            IHasDependencies[] parents = GetParentsWithCache(currentNode);

            /////////////////////////////////////////////////////CONCERNING CHILDREN////////////////////////////////////////////////////////
            //freaky case where the node has EXACTLY 1 child, can merge the node into it's child
            /*    if (children != null && children.Length == 1)
                {
                    //there is there is 1 child which we will merge into this current node
                    string comboStringToAdd = children[0].GetType().Name + Environment.NewLine + children[0];

                    //but only if it is novel
                    if (!dependenciesFoundDuringRecursion.ContainsKey(children[0]))
                    {
                        //it is novel

                        //update the text to incorporate the child
                        currentNodeAsVertex.Text += Environment.NewLine + comboStringToAdd;

                        //record that it is no longer novel
                        dependenciesFoundDuringRecursion.Add(children[0], currentNodeAsVertex);

                        //recurse off of child
                        AddDependenciesToGraphRecursively(children[0], currentNodeAsVertex, dataGraph, depth + 1);
                    }
                }
                else*/
            //normal case
            if (children != null)
                foreach (IHasDependencies child in children)
                {
                    DataVertex childVertex;

                    //add the child
                    if (AddNewChildVertexToParent(currentNodeAsVertex, child, dataGraph, out childVertex))
                        //recurse off of child
                        AddDependenciesToGraphRecursively(child, childVertex, dataGraph, depth + 1);

                    if (Cancelled)
                        return;
                }
            /////////////////////////////////////////////////////END CONCERNING CHILDREN////////////////////////////////////////////////////////


            /////////////////////////////////////////////////////CONCERNING PARENTS////////////////////////////////////////////////////////
            if (parents != null)
                foreach (IHasDependencies parent in parents)
                {
                    DataVertex parentVertex;

                    if (AddNewParentVertexToChild(parent, currentNodeAsVertex, dataGraph, out parentVertex))
                        //add the next level down
                        AddDependenciesToGraphRecursively(parent, parentVertex, dataGraph, depth + 1);


                    if (Cancelled)
                        return;
                }
            /////////////////////////////////////////////////////END CONCERNING PARENTS////////////////////////////////////////////////////////
        }


        private IHasDependencies[] GetParentsWithCache(IHasDependencies currentNode)
        {
            if (!AllowCaching)
                return currentNode.GetObjectsThisDependsOn();

            if (!ParentCache.ContainsKey(currentNode))
                ParentCache[currentNode] = currentNode.GetObjectsThisDependsOn();

            return ParentCache[currentNode];
        }

        private IHasDependencies[] GetChildrenWithCache(IHasDependencies currentNode)
        {
            if (!AllowCaching)
                return currentNode.GetObjectsDependingOnThis();


            if (!ChildCache.ContainsKey(currentNode))
                ChildCache[currentNode] = currentNode.GetObjectsDependingOnThis();

            return ChildCache[currentNode];

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="dataGraph"></param>
        /// <param name="childVertex"></param>
        /// <returns>true if the child is novel (not been added before)</returns>
        private bool AddNewChildVertexToParent(DataVertex parent, IHasDependencies child, GraphExample dataGraph, out DataVertex childVertex)
        {
            if (ShouldSkipType(child))
            {
                childVertex = null;
                return false;
            }

            childVertex = null;
            try
            {
                //if we have not found it previously
                if (!dependenciesFoundDuringRecursion.ContainsKey(child))
                {
                    //add the child
                    //Check if the vertex should be highlighted or not
                    if (_vertexHighlightFilters.ContainsKey(child.GetType().ToString()) &&
                _vertexHighlightFilters[child.GetType().ToString()])
                    {
                        childVertex = new DataVertex(child, _visualiser, true);
                    }
                    else
                        childVertex = new DataVertex(child, _visualiser, false);

                    dataGraph.AddVertex(childVertex);

                    //record that we have found the child
                    dependenciesFoundDuringRecursion.Add(child, childVertex);

                    return true;
                }
                else
                {
                    childVertex = dependenciesFoundDuringRecursion[child];
                    return false;//it was not novel
                }
            }
            finally
            {
                //but always add the relationship anyway
                var dataEdge = new DataEdge(parent, childVertex) { Text = "->" };
                dataGraph.AddEdge(dataEdge);
            }
        }

        private bool ShouldSkipType(IHasDependencies o)
        {
            if (o == null)
                return true;

            Type childType = o.GetType();

            if (_vertexTypeFilters.ContainsKey(childType))
                return !_vertexTypeFilters[childType]; //filter is true to keep it false to throw it away so flipit

            //do not skip it because type was not in the filters dictionary
            return false;
        }

        private bool AddNewParentVertexToChild(IHasDependencies newParent, DataVertex currentVertex, GraphExample dataGraph, out DataVertex newParentVertex)
        {
            if (ShouldSkipType(newParent))
            {
                newParentVertex = null;
                return false;
            }
            newParentVertex = null;
            try
            {
                //if we have found it previously
                if (!dependenciesFoundDuringRecursion.ContainsKey(newParent))
                {
                    //add the parent

                    //Check if the vertex should be highlighted or not
                    if (_vertexHighlightFilters.ContainsKey(newParent.GetType().ToString()) &&
                        _vertexHighlightFilters[newParent.GetType().ToString()])
                    {
                        newParentVertex = new DataVertex(newParent, _visualiser, true);
                    }
                    else
                        newParentVertex = new DataVertex(newParent, _visualiser, false);



                    dataGraph.AddVertex(newParentVertex);

                    //record that we have found the child
                    dependenciesFoundDuringRecursion.Add(newParent, newParentVertex);

                    return true;//it is novel so you should search for it's dependencies too
                }
                else
                {
                    newParentVertex = dependenciesFoundDuringRecursion[newParent];
                    return false;//it is not novel so dont look for it's dependencies
                }
            }
            finally
            {
                var dataEdge = new DataEdge(newParentVertex, currentVertex) { Text = "->" };
                dataGraph.AddEdge(dataEdge);
            }
        }

        public void AddDependenciesTo(GraphExample graph)
        {
            IsWorking = true;
            Cancelled = false;
            DataVertex dataVertex;
            //add the root node
            //Check if the vertex should be highlighted or not
            if (_vertexHighlightFilters.ContainsKey(_root.GetType().ToString()) &&
                _vertexHighlightFilters[_root.GetType().ToString()])
            {
                dataVertex = new DataVertex(_root, _visualiser, true);
            }
            else
                dataVertex = new DataVertex(_root, _visualiser, false);




            graph.AddVertex(dataVertex);
            //record that we have found the root node only
            dependenciesFoundDuringRecursion = new Dictionary<IHasDependencies, DataVertex>();
            dependenciesFoundDuringRecursion.Add(_root, dataVertex);

            //add dependencies recursively
            AddDependenciesToGraphRecursively(_root, dataVertex, graph, 1);
        }

        public void Cancel()
        {
            Cancelled = true;
        }
    }
}
