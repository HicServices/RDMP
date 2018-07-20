using System.Data.Common;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery.ConnectionStringDefaults;

namespace CatalogueLibraryTests.Integration.DataAccess
{
    public class ConnectionStringKeywordAccumulatorTests
    {
        [Test]
        public void TestKeywords()
        {
            var acc = new ConnectionStringKeywordAccumulator(DatabaseType.MYSQLServer);
            acc.AddOrUpdateKeyword("AutoEnlist", "false", ConnectionStringKeywordPriority.SystemDefaultLow);
            
            DbConnectionStringBuilder connectionStringBuilder = new DatabaseHelperFactory(DatabaseType.MYSQLServer).CreateInstance().GetConnectionStringBuilder("localhost","mydb",null,null);

            StringAssert.DoesNotContain("autoenlist",connectionStringBuilder.ConnectionString);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains("autoenlist=False",connectionStringBuilder.ConnectionString);
        }


        [Test]
        public void TestKeywords_OverrideWithLowerPriority_Ignored()
        {
            var acc = new ConnectionStringKeywordAccumulator(DatabaseType.MicrosoftSQLServer);
            acc.AddOrUpdateKeyword("Pooling", "false", ConnectionStringKeywordPriority.SystemDefaultHigh);

            DbConnectionStringBuilder connectionStringBuilder = new DatabaseHelperFactory(DatabaseType.MicrosoftSQLServer).CreateInstance().GetConnectionStringBuilder("localhost", "mydb", null, null);

            StringAssert.DoesNotContain("pooling", connectionStringBuilder.ConnectionString);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains("Pooling=False", connectionStringBuilder.ConnectionString);

            //attempt override with low priority setting it to true (note we flipped case of P just to be a curve ball)
            acc.AddOrUpdateKeyword("pooling","true",ConnectionStringKeywordPriority.SystemDefaultLow);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains("Pooling=False", connectionStringBuilder.ConnectionString);
        }

        [TestCase(DatabaseType.MYSQLServer, "sslmode", "None", "Ssl-Mode","Required")]
        [TestCase(DatabaseType.MicrosoftSQLServer, "AttachDbFilename", @"c:\temp\db", "Initial File Name", @"x:\omg.mdf")]
        [TestCase(DatabaseType.MicrosoftSQLServer, "Asynchronous Processing", "True", "Async", "False")]
        [TestCase(DatabaseType.Oracle, "CONNECTION TIMEOUT", "10", "Connection Timeout", "20")]
        public void TestKeywords_OverrideWithNovelButEquivalentKeyword_Ignored(DatabaseType databaseType, string key1, string value1, string equivalentKey, string value2)
        {
            // SSL Mode , SslMode , Ssl-Mode 
            var acc = new ConnectionStringKeywordAccumulator(databaseType);
            acc.AddOrUpdateKeyword(key1,value1, ConnectionStringKeywordPriority.SystemDefaultHigh);

            DbConnectionStringBuilder connectionStringBuilder = new DatabaseHelperFactory(databaseType).CreateInstance().GetConnectionStringBuilder("localhost", "mydb", null, null);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains(key1 + "=" + value1, connectionStringBuilder.ConnectionString);
            
            //attempt override with low priority setting it to true but also use the alias Ssl-Mode instead of SSL Mode
            acc.AddOrUpdateKeyword(equivalentKey,value2,ConnectionStringKeywordPriority.SystemDefaultLow);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains(key1 + "=" + value1, connectionStringBuilder.ConnectionString, "ConnectionStringKeywordAccumulator did not realise that keywords are equivalent");
        }
        [TestCase(ConnectionStringKeywordPriority.SystemDefaultHigh)] //same as current (still results in override)
        [TestCase(ConnectionStringKeywordPriority.ApiRule)]
        public void TestKeywords_OverrideWithHigherPriority_Respected(ConnectionStringKeywordPriority newPriority)
        {
            var acc = new ConnectionStringKeywordAccumulator(DatabaseType.MicrosoftSQLServer);
            acc.AddOrUpdateKeyword("Pooling", "false", ConnectionStringKeywordPriority.SystemDefaultHigh);

            DbConnectionStringBuilder connectionStringBuilder = new DatabaseHelperFactory(DatabaseType.MicrosoftSQLServer).CreateInstance().GetConnectionStringBuilder("localhost", "mydb", null, null);

            StringAssert.DoesNotContain("pooling", connectionStringBuilder.ConnectionString);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains("Pooling=False", connectionStringBuilder.ConnectionString);

            //attempt override with low priority setting it to true (note we flipped case of P just to be a curve ball)
            acc.AddOrUpdateKeyword("pooling", "true", newPriority);

            acc.EnforceOptions(connectionStringBuilder);

            StringAssert.Contains("Pooling=True", connectionStringBuilder.ConnectionString);
        }


        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void TestKeywords_Invalid(DatabaseType databaseType)
        {
            var acc = new ConnectionStringKeywordAccumulator(databaseType);
          
            Assert.That(() => acc.AddOrUpdateKeyword("FLIBBLE", "false", ConnectionStringKeywordPriority.SystemDefaultLow), Throws.Exception
                .With.Property("Message").ContainsSubstring("FLIBBLE"));
        }
    }
}
