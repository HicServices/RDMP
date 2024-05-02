// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using YamlDotNet.Serialization;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Tracks changes made by the user to one or more objects.  Changes are tracked from
///     the moment the class is constructed until <see cref="TryFinish(IBasicActivateItems)" />
///     completes successfully.  This helps with cancellation
/// </summary>
public class CommitInProgress : IDisposable
{
    private readonly Dictionary<IMapsDirectlyToDatabaseTable, MementoInProgress> originalStates = new();

    public CommitInProgressSettings Settings { get; }

    private readonly IRDMPPlatformRepositoryServiceLocator _locator;
    private readonly List<IRepository> _repositories;
    private readonly ISerializer _serializer;

    /// <summary>
    ///     True when <see cref="TryFinish(IBasicActivateItems)" /> is confirmed to definitely be happening.  Controls
    ///     save suppression
    /// </summary>
    private bool _finishing;

    private bool _isDisposed;

    /// <summary>
    ///     Starts progress towards creating a <see cref="Commit" />.
    /// </summary>
    /// <param name="locator"></param>
    /// <param name="settings"></param>
    public CommitInProgress(IRDMPPlatformRepositoryServiceLocator locator, CommitInProgressSettings settings)
    {
        Settings = settings;

        _locator = locator;

        _repositories = _locator.GetAllRepositories().ToList();
        _serializer = YamlRepository.CreateSerializer(_repositories.SelectMany(r => r.GetCompatibleTypes()).Distinct()
        );

        foreach (var repo in _repositories)
        {
            repo.Deleting += Deleting;
            repo.Inserting += Inserting;

            if (settings.UseTransactions)
                // these get cleaned up in Dispose or TryFinish
                repo.BeginNewTransaction();
        }

        foreach (var t in settings.ObjectsToTrack)
            originalStates.Add(t, new MementoInProgress(t, _serializer.Serialize(t)));
    }

    private void Inserting(object sender, IMapsDirectlyToDatabaseTableEventArgs e)
    {
        if (_finishing)
            return;

        // if we are not in global mode with transactions then this is a long term
        // commit (e.g. user has tab open for half an hour).  Don't hoover up all created/deleted objects
        if (!Settings.UseTransactions) return;

        // how can we be tracking an object that was not created yet?
        if (originalStates.TryGetValue(e.Object, out var state))
            // oh well just pretend it magicked into existence
            state.Type = MementoType.Add;
        else
            // legit new object we didn't know about before
            originalStates.Add(e.Object, new MementoInProgress(e.Object, null)
            {
                Type = MementoType.Add
            });
    }

    private void Deleting(object sender, IMapsDirectlyToDatabaseTableEventArgs e)
    {
        if (_finishing)
            return;

        // if we are not in global mode with transactions then this is a long term
        // commit (e.g. user has tab open for half an hour).  Don't hoover up all created/deleted objects
        if (!Settings.UseTransactions) return;

        // one of the objects we are tracking has been deleted
        if (originalStates.TryGetValue(e.Object, out var inProgress))
        {
            // change our understanding of this object

            if (inProgress.Type == MementoType.Add)
                // ok user created this object during the commit then deleted it again... odd but fair enough
                // pretend it never existed
                originalStates.Remove(e.Object);
            else
                inProgress.Type = MementoType.Delete;
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

    /// <summary>
    ///     Returns a new <see cref="Commit" /> or null if nothing has changed with
    ///     the tracked objects since construction.
    /// </summary>
    /// <returns></returns>
    public Commit TryFinish(IBasicActivateItems activator)
    {
        if (_finishing)
            throw new ObjectDisposedException(
                $"{nameof(CommitInProgress)} has already been successfully finished and shutdown",
                nameof(CommitInProgress));

        if (_isDisposed)
            throw new ObjectDisposedException(nameof(CommitInProgress));

        var changes = new Dictionary<IMapsDirectlyToDatabaseTable, Tuple<MementoInProgress, string>>();

        foreach (var t in originalStates)
        {
            // serialize the current state on finishing into yaml (or use null for deleted objects)
            var newYaml = t.Value.Type == MementoType.Delete ? null : _serializer.Serialize(t.Key);

            //something changed
            if (newYaml != t.Value.OldYaml) changes.Add(t.Key, Tuple.Create(t.Value, newYaml));
        }

        if (!changes.Any())
            // no changes so no need for a Commit
            return null;
        var cataRepo = _locator.CatalogueRepository;

        var description = GetDescription(changes);
        var transaction = Guid.NewGuid();

        if (activator.IsInteractive)
        {
            // object name or count of the number of objects
            var collectionDescription =
                changes.Count == 1 ? changes.Single().Key.ToString() : $"{changes.Count} object(s)";

            if (activator.TypeText(new DialogArgs
                {
                    WindowTitle = transaction.ToString(),
                    TaskDescription = $"Enter a description of what changes you have made to {collectionDescription}"
                }, int.MaxValue, description, out var newDescription, false))
                description = newDescription;
            else
                // user cancelled creating Commit
                return null;
        }

        // We couldn't describe the changes, that's bad...
        if (description == null)
            return null;

        // Ok user has typed in a description (or system generated one) and we are
        // definitely going to do this

        _finishing = true;

        var c = new Commit(cataRepo, transaction, description);

        foreach (var m in changes.OrderBy(c => c.Value.Item1.Order))
            _ = new Memento(cataRepo, c, m.Value.Item1.Type, m.Key, m.Value.Item1.OldYaml, m.Value.Item2);

        // if we created a bunch of db transactions (one per database/server known about) for this commit
        // then we should be letting these changes go ahead
        if (Settings.UseTransactions)
            foreach (var repo in _repositories)
                repo.EndTransaction(true);

        return c;
    }

    private string GetDescription(Dictionary<IMapsDirectlyToDatabaseTable, Tuple<MementoInProgress, string>> changes)
    {
        // no changes
        if (changes.Count == 0)
            return null;

        if (Settings.Description != null) return Settings.Description;

        // we can't summarise changes to multiple objects
        if (changes.Count != 1) return "TODO";

        var kv = changes.Single();
        var props = kv.Value.Item1.GetDiffProperties(kv.Key).ToArray();

        // no visible changes... but yaml is different which is odd.
        // Either way abandon this commit.
        return !props.Any()
            ? null
            : $"Update {kv.Key.GetType().Name} {string.Join(", ", props.Select(p => p.Name).ToArray())}";
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var repo in _repositories)
        {
            repo.Deleting += Deleting;
            repo.Inserting += Inserting;


            if (Settings.UseTransactions && _finishing == false)
                try
                {
                    // Abandon transactions
                    repo.EndTransaction(false);
                }
                catch (NotSupportedException)
                {
                    // ok maybe someone else shut this down somehow... whatever
                }
        }

        _isDisposed = true;
    }
}