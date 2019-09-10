using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;

namespace Tests.Common.Scenarios
{
    /// <summary>
    /// Tests which require two test databases and handles moving data from one to the other
    /// </summary>
    public class FromToDatabaseTests:DatabaseTests
    {
        protected readonly string Suffix;
        protected DiscoveredDatabase From;
        protected DiscoveredDatabase To;

        /// <summary>
        /// The runtime name of <see cref="To"/>.  This is the same as calling <see cref="DiscoveredDatabase.GetRuntimeName()"/>
        /// </summary>
        protected string DatabaseName => To.GetRuntimeName();

        public FromToDatabaseTests(string suffix = "_STAGING")
        {
            Suffix = suffix;
        }

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupFromTo(DatabaseType.MicrosoftSQLServer);
        }

        /// <summary>
        /// Sets up the databases <see cref="From"/> and <see cref="To"/> on the test database server of the given
        /// <see cref="dbType"/>.  This method is automatically called with <see cref="DatabaseType.MicrosoftSQLServer"/>
        /// in <see cref="OneTimeSetUp()"/> (nunit automatically fires it).
        /// </summary>
        /// <param name="dbType"></param>
        protected void SetupFromTo(DatabaseType dbType)
        {
            To = GetCleanedServer(dbType);
            From = To.Server.ExpectDatabase(To.GetRuntimeName() + Suffix);
            
            // ensure the test staging and live databases are empty
            if(!From.Exists())
                From.Create();
            else
                DeleteTables(From);
        }

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();
        }
    }
}
