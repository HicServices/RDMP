using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandDeprecateTests : CommandCliTests
    {
        [Test]
        public void TestDeprecateCommand()
        {
            var c = WhenIHaveA<Catalogue>();

            Assert.IsFalse(c.IsDeprecated);

            Run("Deprecate", $"Catalogue:{c.ID}");

            Assert.IsTrue(c.IsDeprecated);
        }
    }
}
