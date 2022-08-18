// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using YamlDotNet.Serialization;

namespace Rdmp.Core.Curation.Data
{

    /// <summary>
    /// Tracks changes made by the user to one or more objects.  Changes are tracked from
    /// the moment the class is constructed until <see cref="TryFinish(IBasicActivateItems)"/>
    /// completes successfully.  This helps with cancellation
    /// </summary>
    public class CommitInProgress : IDisposable
    {
        /// <summary>
        /// Set to true to block calls to <see cref="ISaveable.SaveToDatabase"/>
        /// on tracked objects until the end of the commit
        /// </summary>
        public bool DelaySaves { get; init; }

        /// <summary>
        /// Set to true to detect ANY object creation or deletion that occurs on any of the 
        /// <see cref="IRepository"/> in the current application scope.  All new/deleted objects
        /// during the lifteime of this object will then be included in any <see cref="Commit"/>
        /// created.  Do not use this for long lifetime <see cref="CommitInProgress"/> e.g. one
        /// associated with an open tab.
        /// </summary>
        public bool TrackInsertsAndDeletes { get; set; }

        Dictionary<IMapsDirectlyToDatabaseTable, MementoInProgress> originalStates = new ();
        
        private IRDMPPlatformRepositoryServiceLocator _locator;
        private IMapsDirectlyToDatabaseTable[] _tracked;
        private IEnumerable<IRepository> _repositories;
        private ISerializer _serializer;

        /// <summary>
        /// We suppress saving on <see cref="_tracked"/> objects until <see cref="TryFinish(IBasicActivateItems)"/>
        /// </summary>
        private Queue<ISaveable> toSave = new();

        int order = 0;

        /// <summary>
        /// True when <see cref="TryFinish(IBasicActivateItems)"/> is confirmed to definetly be happening.  Controls
        /// save suppression
        /// </summary>
        private bool _finishing;

        public CommitInProgress(IRDMPPlatformRepositoryServiceLocator locator,bool trackInsertsAndDeletes, params IMapsDirectlyToDatabaseTable[] track)
        {
            TrackInsertsAndDeletes = trackInsertsAndDeletes;

            _locator = locator;
            _tracked = track;

            _repositories = _locator.GetAllRepositories();
            _serializer = YamlRepository.CreateSerializer(_repositories.SelectMany(r=>r.GetCompatibleTypes()).Distinct()
                );
            foreach(var repo in _repositories)
            {
                repo.Saving += Saving;

                if(trackInsertsAndDeletes)
                {
                    repo.Deleting += Deleting;
                    repo.Inserting += Inserting;
                }
            }

            foreach(var t in track)
            {
                originalStates.Add(t, new MementoInProgress(t,_serializer.Serialize(t)));
                t.PropertyChanged += PropertyChanged;
            }
        }

        private void Inserting(object sender, IMapsDirectlyToDatabaseTableEventArgs e)
        {
            if (_finishing)
                return;

            // how can we be tracking an object that was not created yet?
            if (originalStates.ContainsKey(e.Object))
            {
                // oh well just pretend it magicked into existence
                originalStates[e.Object].Type = MementoType.Add;
                BumpOrder(originalStates[e.Object]);
            }
            else
            {
                // legit new object we didn't know about before
                originalStates.Add(e.Object, new MementoInProgress(e.Object, null)
                {
                    Type = MementoType.Add
                });
                BumpOrder(originalStates[e.Object]);
            }
        }

        private void Deleting(object sender, IMapsDirectlyToDatabaseTableEventArgs e)
        {
            if (_finishing)
                return;

            // one of the objects we are tracking has been deleted
            if (originalStates.ContainsKey(e.Object))
            {
                // change our understanding of this object

                if(originalStates[e.Object].Type == MementoType.Add)
                {
                    // ok user created this object during the commit then deleted it again... odd but fair enough
                    
                    // pretend it never existed
                    originalStates.Remove(e.Object);
                }
                else
                {
                    originalStates[e.Object].Type = MementoType.Delete;
                    BumpOrder(originalStates[e.Object]);
                }
            }
            else
            {
                // an object we are not yet tracking has been deleted
                originalStates.Add(e.Object, new MementoInProgress(e.Object, _serializer.Serialize(e.Object))
                {
                    Type = MementoType.Delete
                });
            }

        }

        private void BumpOrder(MementoInProgress mementoInProgress)
        {
            mementoInProgress.Order = Interlocked.Increment(ref order);
        }

        private void Saving(object sender, SaveEventArgs e)
        {
            // we are not suppressing saves at all
            if (!DelaySaves)
                return;

            // we are in the finishing stage so should stop suppressing saves
            if (_finishing)
                return;

            // if its an object we are tracking and saveable (one would hope so)
            if (_tracked.Contains(e.BeingSaved) && e.BeingSaved is ISaveable s)
            {
                // delay save till TryFinish
                e.Cancel = true;

                if (!toSave.Contains(s))
                    toSave.Enqueue(s);
            }
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var m = (IMapsDirectlyToDatabaseTable)sender;
            BumpOrder(originalStates[m]);
        }

        /// <summary>
        /// Returns a new <see cref="Commit"/> or null if nothing has changed with
        /// the tracked objects since construction.
        /// </summary>
        /// <returns></returns>
        public Commit TryFinish(IBasicActivateItems activator)
        {
            var changes = new Dictionary<IMapsDirectlyToDatabaseTable, Tuple<MementoInProgress,string>>();

            foreach (var t in originalStates)
            {
                // serialize the current state on finishing into yaml (or use null for deleted objects)
                var newYaml = t.Value.Type == MementoType.Delete ? null :  _serializer.Serialize(t.Key);
                
                //something changed
                if(newYaml != t.Value.OldYaml)
                {
                    changes.Add(t.Key, Tuple.Create(t.Value, newYaml));
                }
            }

            if(!changes.Any())
            {
                // no changes so no need for a Commit
                return null;
            }
            var cataRepo = _locator.CatalogueRepository;

            var description = GetDescription(changes);
            var transaction = Guid.NewGuid();

            if (activator.IsInteractive)
            {
                // object name or count of the number of objects
                var collectionDescription = changes.Count == 1 ?
                        changes.Single().Key.ToString() :
                        changes.Count + "object(s)";

                if (activator.TypeText(new DialogArgs
                { 
                    WindowTitle = transaction.ToString(),
                    TaskDescription = $"Enter a description of what changes you have made to {collectionDescription}"
                }, int.MaxValue, description, out var newDescription, false))
                {
                    description = newDescription;
                }
                else
                {
                    // user cancelled creating Commit
                    return null;
                }
            }

            // We couldn't describe the changes, thats bad...
            if (description == null)
                return null;

            // Ok user has typed in a description (or system generated one) and we are
            // definetly going to do this

            // so save all the objects we delayed saving (in the order they asked for saving)
            _finishing = true;

            while (toSave.TryDequeue(out var result))
            {
                result.SaveToDatabase();
            }

            var c = new Commit(cataRepo, transaction, description);

            foreach(var m in changes.OrderBy(c => c.Value.Item1.Order))
            {
                new Memento(cataRepo, c, m.Value.Item1.Type, m.Key, m.Value.Item1.OldYaml, m.Value.Item2);
            }

            return c;
        }

        private string GetDescription(Dictionary<IMapsDirectlyToDatabaseTable, Tuple<MementoInProgress, string>> changes)
        {
            // no changes
            if (changes.Count == 0)
                return null;

            // we can't summarise changes to multiple objects
            if(changes.Count != 1)
            {
                return "TODO";
            }

            var kv = changes.Single();
            var props = kv.Value.Item1.GetDiffProperties(kv.Key).ToArray();

            // no visible changes... but yaml is different which is odd.
            // Either way abandon this commit.
            if(!props.Any())
            {
                return null;
            }

            return $"Update {kv.Key.GetType().Name} {string.Join(", ",props.Select(p => p.Name).ToArray())}";
        }

        public void Dispose()
        {
            foreach (var repo in _repositories)
                repo.Saving -= Saving;
        }
    }
}
