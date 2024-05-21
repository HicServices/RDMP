// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class CommitInProgressTests : DatabaseTests
{
    [Test]
    public void CommitInProgress_CatalogueModify()
    {
        var c = new Catalogue(CatalogueRepository, "Hey");

        using var start = new CommitInProgress(RepositoryLocator, new CommitInProgressSettings(c));

        // no changes but let's spam this for added complexity
        c.SaveToDatabase();
        c.SaveToDatabase();
        c.SaveToDatabase();

        var activator = new ThrowImmediatelyActivator(RepositoryLocator);

        Assert.That(start.TryFinish(activator), Is.Null, "No changes made to Catalogue so expected no commit");

        c.Name = "abadaba";
        c.IsDeprecated = true;
        c.SaveToDatabase();

        var commit = start.TryFinish(activator);
        Assert.That(commit, Is.Not.Null);

        Assert.That(commit.Mementos, Has.Length.EqualTo(1));
        Assert.That(commit.Mementos[0].AfterYaml, Is.Not.EqualTo(commit.Mementos[0].BeforeYaml));
    }

    /// <summary>
    /// Tests that when there is a <see cref="CommitInProgress"/> on object(s) e.g. <see cref="Catalogue"/>
    /// that uses transactions.  Cancelling the <see cref="CommitInProgress"/> will leave everything back
    /// how it was
    /// </summary>
    [Test]
    public void CommitInProgress_TestCancellation()
    {
        var c = new Catalogue(CatalogueRepository, "Hey");

        Assert.That(c.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges),
            "We just created this Catalogue, how can db copy be different?!");

        var start = new CommitInProgress(RepositoryLocator, new CommitInProgressSettings(c)
        {
            UseTransactions = true
        });
        try
        {
            // there is a CommitInProgress on c so db should not have
            c.Name = "abadaba";
            c.IsDeprecated = true;

            Assert.That(c.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.DatabaseCopyDifferent),
                "We have local changes");

            c.SaveToDatabase();

            Assert.That(c.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges),
                "Should be saved inside the transaction");

        }
        finally
        {
            // abandon the commit
            start.Dispose();
        }

        Assert.That(c.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.DatabaseCopyDifferent),
            "With transaction rolled back the Catalogue should now no longer match db state - i.e. be unsaved");

        c.RevertToDatabaseState();

        Assert.That(c.Name, Is.EqualTo("Hey"));
    }
}