using NUnit.Framework;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Modules.Attachers
{
    public class AttacherMEFTest: UnitTests
    {

        [Test]
        public void AttacherMEFCreationTest()
        {
            var types = MEF.GetTypes<IAttacher>().Where(t => !typeof(RemoteAttacher).IsAssignableTo(t)).ToArray();
            List<string> AttacherPaths = types.Select(t => t.FullName).ToList();
            foreach (var path in AttacherPaths)
            {
                Assert.DoesNotThrow(() =>
                {
                    MEF.CreateA<IAttacher>(path);
                });
            }
        }
    }
}
