using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class ExecuteCommandSimilarTests : CommandCliTests
    {
        [Test]
        public void FindSameName_MixedCaps()
        {
            var cata1 = new Catalogue(Repository, "Bob");
            var cata2 = new Catalogue(Repository, "bob");

            var activator = new ThrowImmediatelyActivator(RepositoryLocator);
            var cmd = new ExecuteCommandSimilar(activator, cata1, false);

            Assert.AreEqual(cata2, cmd.Matched.Single());

            cata1.DeleteInDatabase();
            cata2.DeleteInDatabase();
        }

        [Test]
        public void FindDifferent_ColumnInfosSame()
        {
            var c1 = WhenIHaveA<ColumnInfo>();
            var c2 = WhenIHaveA<ColumnInfo>();
            
            var activator = new ThrowImmediatelyActivator(RepositoryLocator);
            var cmd = new ExecuteCommandSimilar(activator, c1, true);

            Assert.IsEmpty(cmd.Matched);

            c1.DeleteInDatabase();
            c2.DeleteInDatabase();

        }

        [Test]
        public void FindDifferent_ColumnInfosDiffer_OnType()
        {
            var c1 = WhenIHaveA<ColumnInfo>();
            c1.Data_type = "varchar(10)";

            var c2 = WhenIHaveA<ColumnInfo>();
            c2.Data_type = "varchar(20)";

            var activator = new ThrowImmediatelyActivator(RepositoryLocator);
            var cmd = new ExecuteCommandSimilar(activator, c1, true);

            Assert.AreEqual(c2, cmd.Matched.Single());

            c1.DeleteInDatabase();
            c2.DeleteInDatabase();
        }
        [Test]
        public void FindDifferent_ColumnInfosDiffer_OnCollation()
        {
            var c1 = WhenIHaveA<ColumnInfo>();
            c1.Collation = "troll doll";

            var c2 = WhenIHaveA<ColumnInfo>();
            c2.Collation = "durdur";

            var activator = new ThrowImmediatelyActivator(RepositoryLocator);
            var cmd = new ExecuteCommandSimilar(activator, c1, true);

            Assert.AreEqual(c2, cmd.Matched.Single());

            c1.DeleteInDatabase();
            c2.DeleteInDatabase();
        }
    }
}
